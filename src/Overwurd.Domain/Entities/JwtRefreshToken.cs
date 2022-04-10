namespace Overwurd.Domain.Entities;

public record JwtRefreshToken(
    string AccessTokenId,
    int UserId,
    string TokenString,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt,
    bool IsRevoked
);