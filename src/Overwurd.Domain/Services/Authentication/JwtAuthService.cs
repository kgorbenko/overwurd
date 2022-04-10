using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Overwurd.Domain.Services.Authentication;

public class JwtAuthService : IJwtAuthService
{
    private readonly TokenValidationParameters tokenValidationParameters;
    private readonly IJwtConfiguration jwtConfiguration;
    private readonly IClaimsIdentityOptions claimsIdentityOptions;
    private readonly IJwtRefreshTokenProvider jwtRefreshTokenProvider;
    private readonly IRandomGuidGenerator randomGuidGenerator;

    public JwtAuthService(IJwtConfiguration jwtConfiguration,
                          TokenValidationParameters tokenValidationParameters,
                          IClaimsIdentityOptions claimsIdentityOptions,
                          IJwtRefreshTokenProvider jwtRefreshTokenProvider,
                          IRandomGuidGenerator randomGuidGenerator)
    {
        this.jwtConfiguration = jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration));
        this.tokenValidationParameters = tokenValidationParameters ?? throw new ArgumentNullException(nameof(tokenValidationParameters));
        this.claimsIdentityOptions = claimsIdentityOptions ?? throw new ArgumentNullException(nameof(claimsIdentityOptions));
        this.jwtRefreshTokenProvider = jwtRefreshTokenProvider ?? throw new ArgumentNullException(nameof(jwtRefreshTokenProvider));
        this.randomGuidGenerator = randomGuidGenerator ?? throw new ArgumentNullException(nameof(randomGuidGenerator));
    }

    public async Task<JwtAccessRefreshTokensPair> GenerateTokensAsync(int userId, IImmutableList<Claim> claims, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = CreateAccessToken(userId, claims.ToArray(), now);
        var accessTokenEncrypted = tokenHandler.WriteToken(jwtSecurityToken);

        var refreshToken = new JwtRefreshToken(
            AccessTokenId: jwtSecurityToken.Id,
            UserId: userId,
            TokenString: randomGuidGenerator.Generate().ToString(),
            ExpiresAt: now.Add(jwtConfiguration.RefreshTokenExpiration),
            CreatedAt: now,
            IsRevoked: false
        );

        await jwtRefreshTokenProvider.RemoveUserTokenAsync(userId, cancellationToken);
        await jwtRefreshTokenProvider.AddTokenAsync(refreshToken, cancellationToken);

        return new JwtAccessRefreshTokensPair(
            AccessToken: accessTokenEncrypted,
            RefreshToken: refreshToken.TokenString,
            AccessTokenExpiresAt: new DateTimeOffset(jwtSecurityToken.ValidTo)
        );
    }

    public async Task<JwtAccessRefreshTokensPair> RefreshAccessTokenAsync(string accessTokenString, string refreshTokenString, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var validatedAccessToken = DecryptAndValidateAccessToken(accessTokenString, tokenValidationParameters);

        var userIdString = validatedAccessToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
        var userId = ParseUserIdHelper.Parse(userIdString);
        var actualRefreshToken = await jwtRefreshTokenProvider.GetUserTokenAsync(userId, cancellationToken);

        var isRefreshTokenValid = IsRefreshTokenValid(actualRefreshToken, refreshTokenString, validatedAccessToken.Id, now);

        if (actualRefreshToken is null || !isRefreshTokenValid)
        {
            throw new InvalidOperationException(
                $"Provided by user #{userId} refresh token is not valid. Refresh token should not be expired or revoked, " +
                "and it should be related with provided user and provided access token");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = GetUserClaims(validatedAccessToken.Claims);
        var newAccessToken = CreateAccessToken(userId, claims, now);
        var newAccessTokenString = tokenHandler.WriteToken(newAccessToken);

        var newRefreshToken = new JwtRefreshToken(
            AccessTokenId: newAccessToken.Id,
            UserId: actualRefreshToken.UserId,
            TokenString: actualRefreshToken.TokenString,
            ExpiresAt: actualRefreshToken.ExpiresAt,
            CreatedAt: actualRefreshToken.CreatedAt,
            IsRevoked: actualRefreshToken.IsRevoked
        );

        await jwtRefreshTokenProvider.RemoveUserTokenAsync(userId, cancellationToken);
        await jwtRefreshTokenProvider.AddTokenAsync(newRefreshToken, cancellationToken);

        return new JwtAccessRefreshTokensPair(
            AccessToken: newAccessTokenString,
            RefreshToken: newRefreshToken.TokenString,
            AccessTokenExpiresAt: new DateTimeOffset(newAccessToken.ValidTo)
        );
    }

    private Claim[] GetUserClaims(IEnumerable<Claim> allClaims)
    {
        var userClaimTypes = new[]
        {
            claimsIdentityOptions.EmailClaimType,
            claimsIdentityOptions.RoleClaimType,
            claimsIdentityOptions.SecurityStampClaimType,
            claimsIdentityOptions.UserIdClaimType,
            claimsIdentityOptions.UserNameClaimType
        }.ToHashSet();

        bool IsUserClaim(Claim claim) => userClaimTypes.Contains(claim.Type);

        return allClaims.Where(IsUserClaim).ToArray();
    }

    private static bool IsRefreshTokenValid(JwtRefreshToken? actualToken,
                                            string providedTokenString,
                                            string accessTokenId,
                                            DateTimeOffset now)
    {
        return actualToken.TokenString == providedTokenString &&
               actualToken.AccessTokenId == accessTokenId &&
               actualToken.ExpiresAt > now &&
               !actualToken.IsRevoked;
    }

    private JwtSecurityToken CreateAccessToken(int userId, Claim[] claims, DateTimeOffset now)
    {
        var defaultClaims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Jti, randomGuidGenerator.Generate().ToString()),
            new(JwtRegisteredClaimNames.Sub, userId.ToString())
        };

        return new(
            issuer: jwtConfiguration.Issuer,
            audience: jwtConfiguration.Audience,
            claims: defaultClaims.Concat(claims),
            expires: now.Add(jwtConfiguration.AccessTokenExpiration).UtcDateTime,
            signingCredentials: new SigningCredentials(
                key: new SymmetricSecurityKey(AuthHelper.GetBytesFromSigningKey(jwtConfiguration.SigningKey)),
                algorithm: jwtConfiguration.SecurityAlgorithmSignature
            )
        );
    }

    private static JwtSecurityToken DecryptAndValidateAccessToken(string token, TokenValidationParameters defaultTokenValidationParameters)
    {
        var tokenValidationParameters = defaultTokenValidationParameters.Clone();
        tokenValidationParameters.ValidateLifetime = false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var _ = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        return (JwtSecurityToken) validatedToken;
    }
}