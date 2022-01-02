using System;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Overwurd.Model.Models;
using Overwurd.Model.Services;
using Overwurd.Web.Options;

namespace Overwurd.Web.Services.Auth
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly JwtConfiguration jwtConfiguration;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly ClaimsIdentityOptions claimsIdentityOptions;
        private readonly IJwtRefreshTokenProvider jwtRefreshTokenProvider;


        public JwtAuthService([NotNull] JwtConfiguration jwtConfiguration,
                              [NotNull] TokenValidationParameters tokenValidationParameters,
                              [NotNull] ClaimsIdentityOptions claimsIdentityOptions,
                              [NotNull] IJwtRefreshTokenProvider jwtRefreshTokenProvider)
        {
            this.jwtConfiguration = jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration));
            this.tokenValidationParameters = tokenValidationParameters ?? throw new ArgumentNullException(nameof(tokenValidationParameters));
            this.claimsIdentityOptions = claimsIdentityOptions ?? throw new ArgumentNullException(nameof(claimsIdentityOptions));
            this.jwtRefreshTokenProvider = jwtRefreshTokenProvider ?? throw new ArgumentNullException(nameof(jwtRefreshTokenProvider));
        }

        public async Task<JwtAuthResult> GenerateTokensAsync(long userId,
                                                             IImmutableList<Claim> claims,
                                                             DateTimeOffset now,
                                                             CancellationToken cancellationToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = CreateAccessToken(userId, jwtConfiguration, claims.ToArray(), now);
            var accessTokenEncrypted = tokenHandler.WriteToken(jwtSecurityToken);

            var refreshToken = new JwtRefreshToken(
                AccessTokenId: jwtSecurityToken.Id,
                UserId: userId,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: now.AddDays(jwtConfiguration.RefreshTokenExpirationInDays),
                CreatedAt: now,
                IsRevoked: false
            );

            await jwtRefreshTokenProvider.RemoveUserTokenAsync(userId, cancellationToken);
            await jwtRefreshTokenProvider.AddTokenAsync(refreshToken, cancellationToken);

            return new JwtAuthResult(
                IsSuccess: true,
                Tokens: new JwtTokenPair(
                    AccessToken: accessTokenEncrypted,
                    RefreshToken: refreshToken.TokenString),
                ErrorMessage: null);
        }

        public async Task<JwtAuthResult> RefreshAccessTokenAsync(string accessTokenString,
                                                                 string refreshTokenString,
                                                                 DateTimeOffset now,
                                                                 CancellationToken cancellationToken)
        {
            var (isSuccess, validatedAccessToken, exception) = DecryptAndValidateAccessToken(accessTokenString, tokenValidationParameters);

            if (!isSuccess)
            {
                return new JwtAuthResult(
                    IsSuccess: false,
                    Tokens: null,
                    ErrorMessage: "Access token is not valid");
            }

            var userIdString = validatedAccessToken.Claims.Single(x => x.Type == claimsIdentityOptions.UserIdClaimType).Value;
            var userId = long.Parse(userIdString);
            var actualRefreshToken = await jwtRefreshTokenProvider.GetUserTokenAsync(userId, cancellationToken);

            if (!IsRefreshTokenValid(actualRefreshToken, refreshTokenString, validatedAccessToken.Id, now))
            {
                return new JwtAuthResult(
                    IsSuccess: false,
                    Tokens: null,
                    ErrorMessage: "Refresh token is not valid");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var newAccessToken = CreateAccessToken(userId, jwtConfiguration, validatedAccessToken.Claims.ToArray(), now);
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

            return new JwtAuthResult(
                IsSuccess: true,
                Tokens: new JwtTokenPair(
                    AccessToken: newAccessTokenString,
                    RefreshToken: newRefreshToken.TokenString),
                ErrorMessage: null);
        }

        private static bool IsRefreshTokenValid(JwtRefreshToken actualToken,
                                                string providedTokenString,
                                                string accessTokenId,
                                                DateTimeOffset now)
        {
            return actualToken is not null &&
                   actualToken.TokenString == providedTokenString &&
                   actualToken.AccessTokenId == accessTokenId &&
                   actualToken.ExpiresAt > now &&
                   !actualToken.IsRevoked;
        }

        private static JwtSecurityToken CreateAccessToken(long userId, JwtConfiguration jwtConfiguration, Claim[] claims, DateTimeOffset now)
        {
            var defaultClaims = new Claim[]
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            };
            return new(
                issuer: jwtConfiguration.Issuer,
                audience: jwtConfiguration.Audience,
                claims: defaultClaims.Concat(claims),
                expires: now.AddMinutes(jwtConfiguration.AccessTokenExpirationInMinutes).UtcDateTime,
                signingCredentials: new SigningCredentials(
                    key: new SymmetricSecurityKey(AuthHelper.GetBytesFromSigningKey(jwtConfiguration.SigningKey)),
                    algorithm: jwtConfiguration.SecurityAlgorithmSignature
                )
            );
        }

        private static (bool IsSuccess, JwtSecurityToken ValidatedToken, Exception Error)
            DecryptAndValidateAccessToken(string token, TokenValidationParameters defaultTokenValidationParameters)
        {
            try
            {
                var tokenValidationParameters = defaultTokenValidationParameters.Clone();
                tokenValidationParameters.ValidateLifetime = false;

                var tokenHandler = new JwtSecurityTokenHandler();
                var _ = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                return (true, (JwtSecurityToken) validatedToken, null);
            }
            catch (Exception exception)
            {
                return (false, null, exception);
            }
        }
    }
}