using System.Threading;
using System.Threading.Tasks;
using Overwurd.Model.Models;

namespace Overwurd.Model.Services
{
    public interface IJwtRefreshTokenProvider
    {
        Task AddTokenAsync(JwtRefreshToken token, CancellationToken cancellationToken);

        Task<JwtRefreshToken> GetUserTokenAsync(long userId, CancellationToken cancellationToken);

        Task RemoveUserTokenAsync(long userId, CancellationToken cancellationToken);
    }
}