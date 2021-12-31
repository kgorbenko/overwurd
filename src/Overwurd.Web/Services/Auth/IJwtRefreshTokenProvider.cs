using System.Threading;
using System.Threading.Tasks;

namespace Overwurd.Web.Services.Auth
{
    public interface IJwtRefreshTokenProvider
    {
        Task AddTokenAsync(JwtRefreshToken token, CancellationToken cancellationToken);

        Task<JwtRefreshToken> GetUserTokenAsync(long userId, CancellationToken cancellationToken);

        Task RemoveUserTokenAsync(long userId, CancellationToken cancellationToken);
    }
}