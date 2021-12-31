using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Overwurd.Model.Tests;

namespace Overwurd.Web.Tests
{
    public abstract class BaseDatabaseDependentTestFixture : OverwurdBaseDatabaseDependentTestFixture
    {
        protected DbContextOptions<ApplicationDbContext> ApplicationContextOptions { get; }

        protected BaseDatabaseDependentTestFixture()
        {
            var connectionString = TestConfiguration.GetConnectionString("DefaultTest");
            ApplicationContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString)
                .Options;
        }
    }
}