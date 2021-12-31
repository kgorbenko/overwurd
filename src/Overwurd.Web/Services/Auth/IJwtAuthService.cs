using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Overwurd.Web.Services.Auth
{
    public interface IJwtAuthService
    {
        Task<JwtAuthResult> GenerateTokensAsync(long userId, IImmutableList<Claim> claims, DateTimeOffset now, CancellationToken cancellationToken);

        Task<JwtAuthResult> RefreshAccessTokenAsync(long userId, string accessTokenString, string refreshTokenString, IImmutableList<Claim> claims, DateTimeOffset now, CancellationToken cancellationToken);
    }
}