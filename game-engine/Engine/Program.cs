using System;
using System.Threading.Tasks;
using Engine.Handlers.Actions;
using Engine.Handlers.Collisions;
using Engine.Handlers.Interfaces;
using Engine.Handlers.Resolvers;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Engine
{
    public class Program
    {
        public static IConfigurationRoot Configuration;

        private static Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            using var serviceScope = host.Services.CreateScope();
            var provider = serviceScope.ServiceProvider;
            var signalRService = provider.GetRequiredService<ISignalRService>();

            signalRService.Startup();

            return host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureServices(GetServiceConfiguration);

        private static void GetServiceConfiguration(HostBuilderContext _, IServiceCollection services)
        {
            RegisterConfigFiles(services);

            // Singletons are instantiated once and remain the same through the lifecycle of the app.
            // Use these for State services
            RegisterSingletonServices(services);

            // Scoped services are created once for each scope
            RegisterScopedServices(services);

            RegisterActionHandlers(services);
            RegisterCollisionHandlers(services);

            RegisterResolvers(services);
        }

        private static void RegisterConfigFiles(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false);
            Configuration = builder.Build();
            services.Configure<EngineConfig>(Configuration);
        }

        private static void RegisterResolvers(IServiceCollection services)
        {
            services.AddTransient<IActionHandlerResolver, ActionHandlerResolver>();
            services.AddTransient<ICollisionHandlerResolver, CollisionHandlerResolver>();
        }

        private static void RegisterScopedServices(IServiceCollection services)
        {
            services.AddScoped<ITickProcessingService, TickProcessingService>();
            services.AddScoped<IActionService, ActionService>();
            services.AddScoped<ICollisionService, CollisionService>();
            services.AddScoped<IWorldObjectGenerationService, WorldObjectGenerationService>();
        }

        private static void RegisterSingletonServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IWorldStateService, WorldStateService>();
            services.AddSingleton<IVectorCalculatorService, VectorCalculatorService>();
            services.AddSingleton<ISignalRService, SignalRService>();
            services.AddSingleton<IEngineService, EngineService>();
        }

        private static void RegisterActionHandlers(IServiceCollection services)
        {
            services.AddTransient<IActionHandler, ForwardActionHandler>();
            services.AddTransient<IActionHandler, StopActionHandler>();
            services.AddTransient<IActionHandler, StartAfterburnerActionHandler>();
            services.AddTransient<IActionHandler, StopAfterburnerActionHandler>();
            services.AddTransient<IActionHandler, FireTorpedoActionHandler>();
            services.AddTransient<IActionHandler, FireSupernovaActionHandler>();
            services.AddTransient<IActionHandler, ActivateShieldActionHandler>();
            services.AddTransient<IActionHandler, DetonateSupernovaActionHandler>();
            services.AddTransient<IActionHandler, FireTeleporterActionHandler>();
            services.AddTransient<IActionHandler, TeleportActionHandler>();
        }

        private static void RegisterCollisionHandlers(IServiceCollection services)
        {
            services.AddTransient<ICollisionHandler, FoodCollisionHandler>();
            services.AddTransient<ICollisionHandler, PlayerCollisionHandler>();
            services.AddTransient<ICollisionHandler, WormholeCollisionHandler>();
            services.AddTransient<ICollisionHandler, GasCloudCollisionHandler>();
            services.AddTransient<ICollisionHandler, AsteroidFieldCollisionHandler>();
            services.AddTransient<ICollisionHandler, SuperfoodCollisionHandler>();
            services.AddTransient<ICollisionHandler, TorpedoCollisionHandler>();
            services.AddTransient<ICollisionHandler, BotToTorpedoCollisionHandler>();
            services.AddTransient<ICollisionHandler, SupernovaPickupCollisionHandler>();
            services.AddTransient<ICollisionHandler, SupernovaBombCollisionHandler>();
            services.AddTransient<ICollisionHandler, TeleporterCollisionHandler>();
        }

        public static void CloseApplication()
        {
            Environment.Exit(0);
        }
    }
}