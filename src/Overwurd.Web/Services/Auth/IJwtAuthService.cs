using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Overwurd.Web.Services.Auth
{
    public record JwtAuthResult(bool IsSuccess, JwtTokenPair Tokens, string ErrorMessage);

    public record JwtTokenPair(string AccessToken, string RefreshToken);

    public interface IJwtAuthService
    {
        Task<JwtAuthResult> GenerateTokensAsync(long userId, IImmutableList<Claim> claims, DateTimeOffset now, CancellationToken cancellationToken);

        Task<JwtAuthResult> RefreshAccessTokenAsync(string accessTokenString, string refreshTokenString, DateTimeOffset now, CancellationToken cancellationToken);
    }
}