using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Overwurd.Model;
using Overwurd.Web.Helpers;

namespace Overwurd.Web.Controllers
{
    [UsedImplicitly]
    public record StatusViewModel(string Version, string Environment, string LastMigration);

    [ApiController]
    [Route("api/status")]
    public class StatusController : Controller
    {
        private readonly OverwurdDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;

        public StatusController([NotNull] IWebHostEnvironment webHostEnvironment,
                                [NotNull] OverwurdDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        [HttpGet]
        public async Task<StatusViewModel> Get()
        {
            return new StatusViewModel(Version: EnvironmentHelper.GetApplicationVersion()?.ToString() ?? "0.0.0.0",
                                       Environment: webHostEnvironment.EnvironmentName,
                                       LastMigration: await EnvironmentHelper.GetLastDatabaseMigrationAsync(dbContext));
        }
    }
}