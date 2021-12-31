using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Overwurd.Model.Tests
{
    public class BaseModelDatabaseDependentTestFixture
    {
        protected IConfiguration TestConfiguration { get; }

        protected DbContextOptions ContextOptions { get; }

        protected BaseModelDatabaseDependentTestFixture()
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            TestConfiguration = new ConfigurationBuilder()
                .SetBasePath(testDirectory)
                .AddJsonFile("appsettings.tests.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = TestConfiguration.GetConnectionString("DefaultTest");
            ContextOptions = new DbContextOptionsBuilder()
                .UseNpgsql(connectionString, options => options.RemoteCertificateValidationCallback((_, _, _, _) => true))
                .Options;
        }

        [SetUp]
        protected async Task PrepareDatabase()
        {
            await using var context = new ModelDbContext(ContextOptions);
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Vocabularies\" RESTART IDENTITY");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Role\" RESTART IDENTITY CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE");
        }
    }
}