using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Web.Services.Auth;
using Overwurd.Web.Tests.EqualityComparers;

namespace Overwurd.Web.Tests
{
    [TestFixture]
    public class TestJwtRefreshTokenProvider : BaseDatabaseDependentTestFixture
    {
        private readonly IEqualityComparer<JwtRefreshToken> refreshTokenComparer = new JwtRefreshTokenComparerForTests();

        [Test]
        public async Task TestGetTokenAsync()
        {
            var guid = Guid.NewGuid().ToString();
            const long userId = 15;
            var now = new DateTimeOffset(year: 2020, month: 2, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var expires = now.AddDays(10);
            var created = now.AddDays(-2);
            var token = new JwtRefreshToken(
                AccessTokenId: "AccessToken1",
                UserId: userId,
                TokenString: guid,
                ExpiresAt: expires,
                CreatedAt: created,
                IsRevoked: false
            );

            await using (var context = new ApplicationDbContext(ApplicationContextOptions))
            {
                await context.AddAsync(token);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ApplicationContextOptions))
            {
                var provider = new JwtRefreshTokenProvider(context);
                var actualToken = provider.GetUserTokenAsync(userId, CancellationToken.None);

                Assert.That(actualToken, Is.EqualTo(token).Using(refreshTokenComparer));
            }
        }
    }
}