using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using NETCoreBot.Enums;
using NETCoreBot.Models;
using NETCoreBot.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NETCoreBot
{
    public class Program
    {
        public static IConfigurationRoot Configuration;

        private static async Task Main(string[] args)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", optional: false);

            Configuration = builder.Build();
            var registrationToken = Environment.GetEnvironmentVariable("Token");
            var environmentIp = Environment.GetEnvironmentVariable("RUNNER_IPV4");
            var ip = !string.IsNullOrWhiteSpace(environmentIp) ? environmentIp : Configuration.GetSection("RunnerIP").Value;
            ip = ip.StartsWith("http://") ? ip : "http://" + ip;

            var port = Configuration.GetSection("RunnerPort");
            
            var url = ip + ":" + port.Value + "/runnerhub";

            var connection = new HubConnectionBuilder()
                                .WithUrl($"{url}")
                                .ConfigureLogging(logging =>
                                {
                                    logging.SetMinimumLevel(LogLevel.Debug);
                                })
                                .WithAutomaticReconnect()
                                .Build();

            var botService = new BotService();

            await connection.StartAsync()
                .ContinueWith(
                    task =>
                    {
                        Console.WriteLine("Connected to Runner");
                        /* Clients should disconnect from the server when the server sends the request to do so. */
                        connection.On<Guid>(
                            "Disconnect",
                            (id) =>
                            {
                                Console.WriteLine("Disconnected:");

                                connection.StopAsync();
                                connection.DisposeAsync();
                            });
                        connection.On<Guid>(
                            "Registered",
                            (id) =>
                            {
                                Console.WriteLine("Registered with the runner");
                                botService.SetBot(
                                    new GameObject
                                    {
                                        Id = id,
                                        GameObjectType = ObjectTypes.Player,
                                        Position = new Position
                                        {
                                            X = 0,
                                            Y = 0
                                        },
                                        Size = 10
                                    });
                            });

                        /* Get the current WorldState along with the last known state of the current client. */
                        connection.On<GameStateDto>(
                            "ReceiveGameState",
                            (gameStateDto) =>
                            {
                                var gameState = new GameState{ World = null, GameObjects = new List<GameObject>(), PlayerGameObjects = new List<GameObject>()};
                                gameState.World = gameStateDto.World;
                                foreach ((var id, List<int> state) in gameStateDto.GameObjects)
                                {
                                    gameState.GameObjects.Add(GameObject.FromStateList(Guid.Parse(id), state));
                                }

                                foreach ((var id, List<int> state) in gameStateDto.PlayerObjects)
                                {
                                    gameState.PlayerGameObjects.Add(GameObject.FromStateList(Guid.Parse(id), state));
                                }
                                botService.SetGameState(gameState);
                            });

                        var token = Environment.GetEnvironmentVariable("REGISTRATION_TOKEN");
                        token = !string.IsNullOrWhiteSpace(token) ? token : Guid.NewGuid().ToString();

                        Thread.Sleep(1000);
                        Console.WriteLine("Registering with the runner...");
                        connection.SendAsync("Register", token, "NetNickName");

                        while (connection.State == HubConnectionState.Connected)
                        {
                            Thread.Sleep(30);
                            Console.WriteLine($"ConState: {connection.State}");
                            Console.WriteLine($"Bot: {botService.GetBot()?.Id.ToString()}");

                            var bot = botService.GetBot();
                            if (bot == null)
                            {
                                continue;
                            }

                            botService.ComputeNextPlayerAction(botService.GetPlayerAction());
                            connection.InvokeAsync("SendPlayerAction", botService.GetPlayerAction());
                        }
                    });
        }
    }
}
