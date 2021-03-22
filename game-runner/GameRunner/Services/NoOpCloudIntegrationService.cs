using System.Threading.Tasks;
using Domain.Services;
using GameRunner.Enums;
using GameRunner.Interfaces;

namespace GameRunner.Services
{
    public class NoOpCloudIntegrationService : ICloudIntegrationService
    {
        private readonly ICloudCallbackFactory cloudCallbackFactory;

        public NoOpCloudIntegrationService(ICloudCallbackFactory cloudCallbackFactory)
        {
            this.cloudCallbackFactory = cloudCallbackFactory;
        }

        public Task Announce(CloudCallbackType callbackType)
        {
            // no-op
            Logger.LogDebug("CloudCallback", $"Cloud Callback called with {callbackType}");
            var cloudCallbackPayload = cloudCallbackFactory.Make(callbackType);
            Logger.LogDebug("CloudCallback", $"Cloud Callback No-opped, Status: {cloudCallbackPayload.MatchStatus}");
            return Task.CompletedTask;
        }
    }
}