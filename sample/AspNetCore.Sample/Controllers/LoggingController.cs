using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecShark.AspNetCore.Extensions;
using RecShark.Extensions;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class LoggingController : ControllerBase
    {
        private readonly ILogger<LoggingController> logger;

        public LoggingController(ILogger<LoggingController> logger)
        {
            this.logger = logger;
        }

        /// <summary> Log parallel </summary>
        [HttpGet]
        public async Task LogParallel()
        {
            void LogNums(int ratio, string name)
            {
                using (logger.WithScope(("name", name)))
                {
                    for (var i = 1; i <= 3; i++)
                    {
                        logger.LogInformation("{number}", ratio*i);
                        Thread.Sleep(500);
                    }
                }
            }

            using (logger.WithScope(("scope", "//")))
            {
                logger.LogInformation("Starting ...");

                await Task.WhenAll(
                    Task.Run(() => LogNums(1, "pos")),
                    Task.Run(() => LogNums(-1, "neg"))
                );

                logger.LogInformation("Finished");
            }
        }
    }
}
