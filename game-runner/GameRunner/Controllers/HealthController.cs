using GameRunner.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameRunner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IRunnerStateService runnerStateService;

        public HealthController(IRunnerStateService runnerStateService)
        {
            this.runnerStateService = runnerStateService;
        }

        [HttpGet("Runner")]
        public IActionResult GetRunnerHealth() => Ok("Runner is available");

        [HttpGet("Engine")]
        public IActionResult GetEngineHealth()
        {
            var engine = runnerStateService.GetEngine();
            if (engine != null)
            {
                return Ok("Engine is available");
            }

            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }

        [HttpGet("Logger")]
        public IActionResult GetLoggerHealth()
        {
            var logger = runnerStateService.GetLogger();
            if (logger != null)
            {
                return Ok("Logger is available");
            }

            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }
    }
}