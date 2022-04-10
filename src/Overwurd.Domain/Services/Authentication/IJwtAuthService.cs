using System.Security.Claims;

namespace Overwurd.Domain.Services.Authentication;

public interface IJwtAuthService
{
    Task<JwtAccessRefreshTokensPair> GenerateTokensAsync(int userId, IImmutableList<Claim> claims, DateTimeOffset now, CancellationToken cancellationToken);

    Task<JwtAccessRefreshTokensPair> RefreshAccessTokenAsync(string accessTokenString, string refreshTokenString, DateTimeOffset now, CancellationToken cancellationToken);
}