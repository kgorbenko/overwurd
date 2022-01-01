using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Overwurd.Model.Models;
using Overwurd.Model.Services;
using Overwurd.Model.Tests.EqualityComparers;

namespace Overwurd.Model.Tests.Services
{
    [TestFixture]
    public class TestJwtRefreshTokenProvider : BaseModelDatabaseDependentTestFixture
    {
        private static readonly IEqualityComparer<JwtRefreshToken> refreshTokenComparer = new JwtRefreshTokenComparerForTests();

        private async Task<User> SaveUserAsync(string login)
        {
            await using var context = new ModelDbContext(ContextOptions);
            var user = new User { Login = login };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        private async Task SaveRefreshTokenAsync(JwtRefreshToken token)
        {
            await using var context = new ModelDbContext(ContextOptions);
            await context.JwtRefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();
        }

        [Test]
        public async Task TestGetUserTokenAsync()
        {
            var user = await SaveUserAsync("User1");

            var date = new DateTimeOffset(year: 2020, month: 2, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var token = new JwtRefreshToken(
                AccessTokenId: "AccessToken1",
                UserId: user.Id,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: date.AddDays(10),
                CreatedAt: date.AddDays(-2),
                IsRevoked: false
            );

            await SaveRefreshTokenAsync(token);

            await using var context = new ModelDbContext(ContextOptions);
            var provider = new JwtRefreshTokenProvider(context);
            var actualToken = await provider.GetUserTokenAsync(user.Id, CancellationToken.None);

            Assert.That(actualToken, Is.EqualTo(token).Using(refreshTokenComparer));
            Assert.That(context.ChangeTracker.Entries<JwtRefreshToken>(), Is.Empty);
        }

        [Test]
        public async Task TestRemoveUserTokenAsync()
        {
            var user = await SaveUserAsync("User1");

            var date = new DateTimeOffset(year: 2020, month: 2, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var token = new JwtRefreshToken(
                AccessTokenId: "AccessToken1",
                UserId: user.Id,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: date.AddDays(10),
                CreatedAt: date.AddDays(-2),
                IsRevoked: false
            );

            await SaveRefreshTokenAsync(token);

            await using (var context = new ModelDbContext(ContextOptions))
            {
                var provider = new JwtRefreshTokenProvider(context);
                await provider.RemoveUserTokenAsync(user.Id, CancellationToken.None);
            }

            await using (var context = new ModelDbContext(ContextOptions))
            {
                var actualTokens = await context.JwtRefreshTokens.ToArrayAsync();
                Assert.That(actualTokens, Is.Empty);
            }
        }

        [Test]
        public async Task RemoveUserTokenNonExistentUser()
        {
            var user = await SaveUserAsync("User1");

            var date = new DateTimeOffset(year: 2020, month: 2, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var token = new JwtRefreshToken(
                AccessTokenId: "AccessToken1",
                UserId: user.Id,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: date.AddDays(10),
                CreatedAt: date.AddDays(-2),
                IsRevoked: false
            );

            await SaveRefreshTokenAsync(token);

            await using var context = new ModelDbContext(ContextOptions);
            var provider = new JwtRefreshTokenProvider(context);
            await provider.RemoveUserTokenAsync(user.Id + 1, CancellationToken.None);

            var actualTokens = await context.JwtRefreshTokens.ToArrayAsync();
            Assert.That(actualTokens, Is.EqualTo(new[] { token }).Using(refreshTokenComparer));
        }

        [Test]
        public async Task TestAddTokenAsync()
        {
            var user = await SaveUserAsync("User1");

            var date = new DateTimeOffset(year: 2020, month: 2, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var token = new JwtRefreshToken(
                AccessTokenId: "AccessToken1",
                UserId: user.Id,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: date.AddDays(10),
                CreatedAt: date.AddDays(-2),
                IsRevoked: false
            );

            await using (var context = new ModelDbContext(ContextOptions))
            {
                var provider = new JwtRefreshTokenProvider(context);
                await provider.AddTokenAsync(token, CancellationToken.None);
            }

            await using (var context = new ModelDbContext(ContextOptions))
            {
                var actualTokens = await context.JwtRefreshTokens.ToArrayAsync();
                Assert.That(actualTokens, Is.EqualTo(new[] { token }).Using(refreshTokenComparer));
            }
        }

        [Test]
        public async Task TestAddDuplicatingByUserToken()
        {
            var user = await SaveUserAsync("User1");

            var date1 = new DateTimeOffset(year: 2020, month: 2, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var token1 = new JwtRefreshToken(
                AccessTokenId: "AccessToken1",
                UserId: user.Id,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: date1.AddDays(10),
                CreatedAt: date1.AddDays(-2),
                IsRevoked: false
            );

            await SaveRefreshTokenAsync(token1);

            var date2 = new DateTimeOffset(year: 2020, month: 2, day: 2, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);
            var token2 = new JwtRefreshToken(
                AccessTokenId: "AccessToken2",
                UserId: user.Id,
                TokenString: Guid.NewGuid().ToString(),
                ExpiresAt: date2.AddDays(10),
                CreatedAt: date2.AddDays(-2),
                IsRevoked: false
            );

            await using var context = new ModelDbContext(ContextOptions);
            var provider = new JwtRefreshTokenProvider(context);

            Assert.ThrowsAsync<DbUpdateException>(async () => await provider.AddTokenAsync(token2, CancellationToken.None));
        }
    }
}