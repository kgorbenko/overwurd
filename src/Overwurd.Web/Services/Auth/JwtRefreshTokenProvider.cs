using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Overwurd.Web.Services.Auth
{
    public class JwtRefreshTokenProvider : IJwtRefreshTokenProvider
    {
        private readonly AuthDbContext dbContext;

        public JwtRefreshTokenProvider([NotNull] AuthDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddTokenAsync(JwtRefreshToken token, CancellationToken cancellationToken)
        {
            await dbContext.JwtRefreshTokens.AddAsync(token, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<JwtRefreshToken> GetUserTokenAsync(long userId, CancellationToken cancellationToken)
        {
            return await dbContext.JwtRefreshTokens
                                  .Where(x => x.UserId == userId)
                                  .AsNoTracking()
                                  .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task RemoveUserTokenAsync(long userId, CancellationToken cancellationToken)
        {
            var userToken = await dbContext.JwtRefreshTokens
                                           .FindAsync(new object[] { userId }, cancellationToken: cancellationToken);

            if (userToken is not null)
            {
                dbContext.JwtRefreshTokens.RemoveRange(userToken);
            }
        }
    }
}