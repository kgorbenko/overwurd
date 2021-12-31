using System;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.IdentityModel.Tokens;
using Overwurd.Web.Options;

namespace Overwurd.Web.Services.Auth
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly JwtConfiguration jwtConfiguration;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly IJwtRefreshTokenProvider jwtRefreshTokenProvider;

        public JwtAuthService([NotNull] JwtConfiguration jwtConfiguration,
                              [NotNull] TokenValidationParameters tokenValidationParameters,
                              [NotNull] IJwtRefreshTokenProvider jwtRefreshTokenProvider)
        {
            this.jwtConfiguration = jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration));
            this.tokenValidationParameters = tokenValidationParameters ?? throw new ArgumentNullException(nameof(tokenValidationParameters));
            this.jwtRefreshTokenProvider = jwtRefreshTokenProvider ?? throw new ArgumentNullException(nameof(jwtRefreshTokenProvider));
        }

        public async Task<JwtAuthResult> GenerateTokensAsync(long userId,
                                                             IImmutableList<Claim> claims,
                                                             DateTimeOffset now,
                                                             CancellationToken cancellationToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = CreateAccessToken(jwtConfiguration, claims.ToArray(), now);
            var accessTokenEncrypted = tokenHandler.WriteToken(jwtSecurityToken);

            var refreshToken = new JwtRefreshToken(
                AccessTokenId: jwtSecurityToken.Id,
                UserId: userId,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: now.AddMinutes(jwtConfiguration.RefreshTokenExpirationInDays),
                CreatedAt: now,
                IsRevoked: false
            );

            await jwtRefreshTokenProvider.RemoveUserTokenAsync(userId, cancellationToken);
            await jwtRefreshTokenProvider.AddTokenAsync(refreshToken, cancellationToken);

            return new JwtAuthResult(
                AccessToken: accessTokenEncrypted,
                RefreshToken: refreshToken.TokenString);
        }

        public async Task<JwtAuthResult> RefreshAccessTokenAsync(long userId,
                                                                 string accessTokenString,
                                                                 string refreshTokenString,
                                                                 IImmutableList<Claim> claims,
                                                                 DateTimeOffset now,
                                                                 CancellationToken cancellationToken)
        {
            var validatedAccessToken = DecryptAndValidateAccessToken(accessTokenString, tokenValidationParameters);

            if (IsAccessTokenValid(validatedAccessToken, now))
            {
                return null;
            }

            var actualRefreshToken = await jwtRefreshTokenProvider.GetUserTokenAsync(userId, cancellationToken);

            if (!IsRefreshTokenValid(actualRefreshToken, refreshTokenString, validatedAccessToken.Id, now))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var newAccessToken = CreateAccessToken(jwtConfiguration, claims.ToArray(), now);
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
                AccessToken: newAccessTokenString,
                RefreshToken: newRefreshToken.TokenString);
        }

        private static bool IsAccessTokenValid(JwtSecurityToken token, DateTimeOffset now)
        {
            return token is not null && token.ValidTo < now.UtcDateTime;
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

        private static JwtSecurityToken CreateAccessToken(JwtConfiguration jwtConfiguration, Claim[] claims, DateTimeOffset now) =>
            new(
                issuer: jwtConfiguration.Issuer,
                audience: jwtConfiguration.Audience,
                claims: claims,
                expires: now.AddMinutes(jwtConfiguration.AccessTokenExpirationInMinutes).UtcDateTime,
                signingCredentials: new SigningCredentials(
                    key: new SymmetricSecurityKey(AuthHelper.GetBytesFromSigningKey(jwtConfiguration.SigningKey)),
                    algorithm: jwtConfiguration.SecurityAlgorithmSignature
                )
            );

        private static JwtSecurityToken DecryptAndValidateAccessToken(string token, TokenValidationParameters defaultTokenValidationParameters)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = defaultTokenValidationParameters.ValidateIssuer,
                    ValidIssuer = defaultTokenValidationParameters.ValidIssuer,
                    ValidateIssuerSigningKey = defaultTokenValidationParameters.ValidateIssuerSigningKey,
                    IssuerSigningKey = defaultTokenValidationParameters.IssuerSigningKey,
                    ValidateAudience = defaultTokenValidationParameters.ValidateAudience,
                    ValidAudience = defaultTokenValidationParameters.ValidAudience,
                    RequireExpirationTime = defaultTokenValidationParameters.RequireExpirationTime,
                    ValidateLifetime = false,
                    ClockSkew = defaultTokenValidationParameters.ClockSkew,
                    ValidAlgorithms = defaultTokenValidationParameters.ValidAlgorithms
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var _ = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                return (JwtSecurityToken) validatedToken;
            }
            catch
            {
                return null;
            }
        }
    }
}