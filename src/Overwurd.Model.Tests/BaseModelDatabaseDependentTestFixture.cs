using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Overwurd.Model.Tests
{
    public class BaseModelDatabaseDependentTestFixture
    {
        protected IConfiguration TestConfiguration { get; }

        protected DbContextOptions<ModelDbContext> ContextOptions { get; }

        protected BaseModelDatabaseDependentTestFixture()
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            TestConfiguration = new ConfigurationBuilder()
                .SetBasePath(testDirectory)
                .AddJsonFile("appsettings.tests.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = TestConfiguration.GetConnectionString("DefaultTest");
            ContextOptions = new DbContextOptionsBuilder<ModelDbContext>()
                .UseNpgsql(connectionString)
                .Options;
        }

        protected virtual async Task CleanDatabase()
        {
            await using var context = new ModelDbContext(ContextOptions);
            await context.Database.ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS {ModelDbContext.SchemaName} CASCADE");
        }

        [SetUp]
        protected async Task PrepareDatabase()
        {
            await CleanDatabase();
            await using var context = new ModelDbContext(ContextOptions);
            await context.Database.EnsureCreatedAsync();
        }
    }
}