using System.Threading;
using System.Threading.Tasks;
using Overwurd.Model.Models;

namespace Overwurd.Model.Services;

public interface IJwtRefreshTokenProvider
{
    Task AddTokenAsync(JwtRefreshToken token, CancellationToken cancellationToken);

    Task<JwtRefreshToken> GetUserTokenAsync(int userId, CancellationToken cancellationToken);

    Task RemoveUserTokenAsync(int userId, CancellationToken cancellationToken);
}