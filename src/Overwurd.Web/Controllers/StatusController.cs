using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Overwurd.Web.Helpers;
using Overwurd.Web.ViewModels;

namespace Overwurd.Web.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : Controller
    {
        [HttpGet]
        public StatusViewModel Get([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            return new StatusViewModel(Version: EnvironmentHelper.GetApplicationVersion()?.ToString() ?? "0.0.0.0",
                                       Environment: webHostEnvironment.EnvironmentName);
        }
    }
}