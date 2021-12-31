using System;

namespace Overwurd.Web.Services.Auth
{
    public record JwtRefreshToken(
        string AccessTokenId,
        long UserId,
        string TokenString,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CreatedAt,
        bool IsRevoked
    );
}