using System;

namespace Overwurd.Model.Models
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