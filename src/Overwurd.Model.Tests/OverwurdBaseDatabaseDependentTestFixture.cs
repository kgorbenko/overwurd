using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Overwurd.Model.Tests
{
    public class OverwurdBaseDatabaseDependentTestFixture
    {
        protected DbContextOptions<OverwurdDbContext> ContextOptions { get; }

        protected OverwurdBaseDatabaseDependentTestFixture()
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            var config = new ConfigurationBuilder()
                .SetBasePath(testDirectory)
                .AddJsonFile("appsettings.tests.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("TestOverwurdDatabase");
            ContextOptions = new DbContextOptionsBuilder<OverwurdDbContext>()
                .UseNpgsql(connectionString)
                .Options;
        }

        [SetUp]
        protected async Task PrepareDatabase()
        {
            await using var context = new OverwurdDbContext(ContextOptions);
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Vocabularies\" RESTART IDENTITY");
        }
    }
}