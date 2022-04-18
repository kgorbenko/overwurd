namespace Overwurd.Domain.Services.Authentication;

public record JwtAccessRefreshTokensPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);