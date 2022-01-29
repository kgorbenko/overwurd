using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Overwurd.Web.Services.Auth;

public record JwtTokenPairData(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);

public interface IJwtAuthService
{
    Task<JwtTokenPairData> GenerateTokensAsync(int userId, IImmutableList<Claim> claims, DateTimeOffset now, CancellationToken cancellationToken);

    Task<JwtTokenPairData> RefreshAccessTokenAsync(string accessTokenString, string refreshTokenString, DateTimeOffset now, CancellationToken cancellationToken);
}