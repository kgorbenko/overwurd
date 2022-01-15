using System;

namespace Overwurd.Model.Models
{
    public record JwtRefreshToken(
        string AccessTokenId,
        int UserId,
        string TokenString,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CreatedAt,
        bool IsRevoked
    );
}