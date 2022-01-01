using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model.Services
{
    public class JwtRefreshTokenProvider : IJwtRefreshTokenProvider
    {
        private readonly ModelDbContext dbContext;

        public JwtRefreshTokenProvider([NotNull] ModelDbContext dbContext)
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
                dbContext.JwtRefreshTokens.Remove(userToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}