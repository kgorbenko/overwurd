using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Overwurd.Model.Tests;

namespace Overwurd.Web.Tests
{
    public class BaseApplicationDatabaseDependentTestFixture : BaseModelDatabaseDependentTestFixture
    {
        [SetUp]
        protected new async Task PrepareDatabase()
        {
            await using var context = new ApplicationDbContext(ContextOptions);
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"JwtRefreshTokens\" RESTART IDENTITY");
        }
    }
}