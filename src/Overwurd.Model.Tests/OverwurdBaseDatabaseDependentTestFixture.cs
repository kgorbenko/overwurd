using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Overwurd.Model.Tests
{
    public class OverwurdBaseDatabaseDependentTestFixture
    {
        protected IConfiguration TestConfiguration { get; }

        protected DbContextOptionsBuilder ContextOptionsBuilder { get; }

        protected OverwurdBaseDatabaseDependentTestFixture()
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            TestConfiguration = new ConfigurationBuilder()
                .SetBasePath(testDirectory)
                .AddJsonFile("appsettings.tests.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = TestConfiguration.GetConnectionString("DefaultTest");
            ContextOptionsBuilder = new DbContextOptionsBuilder()
                .UseNpgsql(connectionString, options => options.RemoteCertificateValidationCallback((_, _, _, _) => true));
        }

        [SetUp]
        protected async Task PrepareDatabase()
        {
            await using var context = new ModelDbContext(ContextOptionsBuilder);
            await context.Database.MigrateAsync();
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Vocabularies\" RESTART IDENTITY");
        }
    }
}