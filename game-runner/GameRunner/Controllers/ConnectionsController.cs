using System;
using Domain.Services;
using GameRunner.Enums;
using GameRunner.Interfaces;
using GameRunner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GameRunner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionsController : ControllerBase
    {
        private readonly IRunnerStateService runnerStateService;
        private readonly IHubContext<RunnerHub> hubContext;
        private readonly ICloudIntegrationService cloudIntegrationService;

        public ConnectionsController(
            IRunnerStateService runnerStateService,
            IHubContext<RunnerHub> hubContext,
            ICloudIntegrationService cloudIntegrationService)
        {
            this.runnerStateService = runnerStateService;
            this.hubContext = hubContext;
            this.cloudIntegrationService = cloudIntegrationService;
        }

        [HttpPost("engine")]
        public IActionResult UpdateEngineConnectionStatus([FromBody] ConnectionInformation connectionInformation)
        {
            if (connectionInformation.Status == ConnectionStatus.Disconnected)
            {
                var failReason = $"Engine informed of Disconnect. Reason: {connectionInformation.Reason}.\n Disconnecting all clients and stopping";
                Logger.LogError(
                    "Connections",
                    failReason);
                runnerStateService.FailureReason = failReason;
                cloudIntegrationService.Announce(CloudCallbackType.Failed);
                hubContext.Clients.All.SendAsync("Disconnect", new Guid());
                runnerStateService.StopApplication();
            }

            return Ok();
        }

        [HttpPost("logger")]
        public IActionResult UpdateLoggerConnectionStatus([FromBody] ConnectionInformation connectionInformation)
        {
            if (connectionInformation.Status == ConnectionStatus.Disconnected)
            {
                var failReason = $"Logger informed of Disconnect. Reason: {connectionInformation.Reason}.\n Disconnecting all clients and stopping";
                Logger.LogError(
                    "Connections",
                    failReason);
                runnerStateService.FailureReason = failReason;
                cloudIntegrationService.Announce(CloudCallbackType.Failed);
                hubContext.Clients.All.SendAsync("Disconnect", new Guid());
                runnerStateService.StopApplication();
            }

            return Ok();
        }
    }
}