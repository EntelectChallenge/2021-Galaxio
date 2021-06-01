using System.Text.Json.Serialization;
using GameRunner.Interfaces;
using GameRunner.Models;
using GameRunner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameRunner
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR(
                    o =>
                    {
                        o.EnableDetailedErrors = true;
                        o.MaximumReceiveMessageSize = 2000000;
                    })
                .AddJsonProtocol(options => { options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()); })
                .AddMessagePackProtocol();
            services.AddCors();
            var runnerConfig = Configuration.GetSection("RunnerConfig");
            services.Configure<RunnerConfig>(runnerConfig);

            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IRunnerStateService, RunnerStateService>();
            services.AddSingleton<IEnvironmentService, EnvironmentService>();
            services.AddSingleton<ITimerService, TimerService>();

            services.AddTransient<ICloudCallbackFactory, CloudCallbackFactory>();

            var envarService = new EnvironmentService();
            if (envarService.IsCloud)
            {
                services.AddTransient<ICloudIntegrationService, CloudIntegrationService>();
            }
            else
            {
                services.AddTransient<ICloudIntegrationService, NoOpCloudIntegrationService>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseCors(builder => builder.AllowAnyOrigin());

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapHub<RunnerHub>("/runnerhub",
                        options =>
                        {
                            options.ApplicationMaxBufferSize = 200000000;
                            options.TransportMaxBufferSize = 200000000;
                        });
                });
        }
    }
}