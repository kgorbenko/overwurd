using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Overwurd.Model.Tests
{
    public class BaseModelDatabaseDependentTestFixture
    {
        protected IConfiguration TestConfiguration { get; }

        protected DbContextOptions<ApplicationDbContext> ContextOptions { get; }

        protected BaseModelDatabaseDependentTestFixture()
        {
            var testDirectory = TestContext.CurrentContext.TestDirectory;
            TestConfiguration = new ConfigurationBuilder()
                .SetBasePath(testDirectory)
                .AddJsonFile("appsettings.tests.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = TestConfiguration.GetConnectionString("DefaultTest");
            ContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                             .UseNpgsql(connectionString, builder =>
                             {
                                 builder.MigrationsHistoryTable(ApplicationDbContext.MigrationsHistoryTableName,
                                                                ApplicationDbContext.SchemaName);
                                 builder.RemoteCertificateValidationCallback((_, _, _, _) => true);
                             })
                             .Options;
        }

        protected virtual async Task CleanDatabase()
        {
            await using var context = new ApplicationDbContext(ContextOptions);

            if (await context.Database.CanConnectAsync())
            {
                await context.Database.ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS {ApplicationDbContext.SchemaName} CASCADE");
            }
        }

        [SetUp]
        protected async Task PrepareDatabase()
        {
            await CleanDatabase();
            await using var context = new ApplicationDbContext(ContextOptions);
            await context.Database.MigrateAsync();
            await context.Database.EnsureCreatedAsync();
        }
    }
}