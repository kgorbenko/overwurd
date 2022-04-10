namespace Overwurd.Domain.Services;

public interface IJwtRefreshTokenProvider
{
    Task AddTokenAsync(JwtRefreshToken token, CancellationToken cancellationToken = default);

    Task<JwtRefreshToken?> GetUserTokenAsync(int userId, CancellationToken cancellationToken = default);

    Task RemoveUserTokenAsync(int userId, CancellationToken cancellationToken = default);
}