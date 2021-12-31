using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Web.Services.Auth;
using Overwurd.Web.Tests.EqualityComparers;

namespace Overwurd.Web.Tests
{
    [TestFixture]
    public class TestJwtRefreshTokenProvider : BaseApplicationDatabaseDependentTestFixture
    {
        private readonly IEqualityComparer<JwtRefreshToken> refreshTokenComparer = new JwtRefreshTokenComparerForTests();

        [Test]
        public async Task TestGetTokenAsync()
        {
            var user = new User { Login = "TestUser1" };
            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            var guid = Guid.NewGuid().ToString();
            var now = new DateTimeOffset(year: 2020, month: 2, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var expires = now.AddDays(10);
            var created = now.AddDays(-2);
            var token = new JwtRefreshToken(
                AccessTokenId: "AccessToken1",
                UserId: user.Id,
                TokenString: guid,
                ExpiresAt: expires,
                CreatedAt: created,
                IsRevoked: false
            );

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                await context.JwtRefreshTokens.AddAsync(token);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(ContextOptions))
            {
                var provider = new JwtRefreshTokenProvider(context);
                var actualToken = await provider.GetUserTokenAsync(user.Id, CancellationToken.None);

                Assert.That(actualToken, Is.EqualTo(token).Using(refreshTokenComparer));
            }
        }
    }
}