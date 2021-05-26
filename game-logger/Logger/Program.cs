using System;
using System.Threading.Tasks;
using Logger.Interfaces;
using Logger.Models;
using Logger.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Logger
{
    public class Program
    {
        public static IConfigurationRoot Configuration;

        private static Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            using var serviceScope = host.Services.CreateScope();
            var provider = serviceScope.ServiceProvider;
            var logger = provider.GetRequiredService<ILogRecorderService>();

            logger.Startup();

            return host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices(GetServiceConfiguration);

        private static void GetServiceConfiguration(HostBuilderContext _, IServiceCollection services)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false);
            Configuration = builder.Build();
            services.Configure<LoggerConfig>(Configuration);
            // Singletons are instantiated once and remain the same through the lifecycle of the app.

            // Transient are created each time they are called
            services.AddSingleton<ILogRecorderService, LogRecorder>();

            // Scope services are created once for each scope
        }

        public static void CloseApplication()
        {
            Environment.Exit(0);
        }
    }
}