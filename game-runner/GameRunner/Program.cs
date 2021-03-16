using System;
using Domain.Services;
using GameRunner.Enums;
using GameRunner.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();

                using var serviceScope = host.Services.CreateScope();
                var provider = serviceScope.ServiceProvider;
                var cloudIntegrationService = provider.GetRequiredService<ICloudIntegrationService>();
                cloudIntegrationService.Announce(CloudCallbackType.Initializing);
                host.Run();
            }
            catch (OperationCanceledException e)
            {
                Logger.LogDebug("Main", e.Message);
            }
            catch (Exception e)
            {
                Logger.LogError("Main", e.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        // TODO we need to make this better and include https support once we get certs sorted
                        webBuilder.UseUrls("http://*:5000");
                    });
    }
}