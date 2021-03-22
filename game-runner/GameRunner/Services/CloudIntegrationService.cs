using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Domain.Services;
using GameRunner.Enums;
using GameRunner.Interfaces;

namespace GameRunner.Services
{
    public class CloudIntegrationService : ICloudIntegrationService
    {
        private readonly IEnvironmentService environmentService;
        private readonly HttpClient httpClient;
        private readonly ICloudCallbackFactory cloudCallbackFactory;

        public CloudIntegrationService(IEnvironmentService environmentService, ICloudCallbackFactory cloudCallbackFactory)
        {
            this.environmentService = environmentService;
            this.cloudCallbackFactory = cloudCallbackFactory;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Api-Key", environmentService.ApiKey);
        }

        public async Task Announce(CloudCallbackType callbackType)
        {
            var cloudCallbackPayload = cloudCallbackFactory.Make(callbackType);
            Logger.LogInfo("CloudCallback", $"Cloud Callback Initiated, Status: {cloudCallbackPayload.MatchStatus}, Callback player Count: {cloudCallbackPayload.Players?.Count}");
            try
            {
                var result = await httpClient.PostAsync(environmentService.ApiUrl, cloudCallbackPayload, new JsonMediaTypeFormatter());
                if (!result.IsSuccessStatusCode)
                {
                    Logger.LogWarning("CloudCallback", $"Received non-success status code from cloud callback. Code: {result.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Logger.LogError("CloudCallback", $"Failed to make cloud callback with error: {e.Message}");
            }
        }
    }
}