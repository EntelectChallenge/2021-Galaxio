using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using NETCoreBot.Enums;
using NETCoreBot.Models;
using NETCoreBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            var ip = !string.IsNullOrWhiteSpace(environmentIp)
                ? environmentIp
                : Configuration.GetSection("RunnerIP").Value;

            var port = Configuration.GetSection("RunnerPort");

            ip = ip.StartsWith("http://") ? ip : "http://" + ip;

                var url = ip + ":" + port.Value + "/runnerhub";

            var connection = new HubConnectionBuilder().WithUrl($"{url}").WithAutomaticReconnect().Build();

            var botService = new BotService();

            await connection.StartAsync()
                .ContinueWith(
                    task =>
                    {
                        Console.WriteLine("Connected to Runner");
                        /* Clients should disconnect from the server when the server sends the request to do so. */
                        connection.On<Guid>(
                            "Disconnect",
                            (Id) =>
                            {
                                OnDisconnect(connection);
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
                                var alive = botService.ComputeNextPlayerAction(botService.GetPlayerAction());
                                if (!alive)
                                {
                                    OnDisconnect(connection);
                                }
                                connection.InvokeAsync("SendPlayerAction", botService.GetPlayerAction());
                            });

                        var token = Environment.GetEnvironmentVariable("REGISTRATION_TOKEN");
                        token = !string.IsNullOrWhiteSpace(token) ? token : Guid.NewGuid().ToString();

                        Thread.Sleep(1000);
                        Console.WriteLine($"Registering with the runner: {token}");
                        connection.SendAsync("Register", token, "ReferenceBot");
                    });
            while (connection.State == HubConnectionState.Connected)
            {
                continue;
            }
            Console.WriteLine("Stopping.");
        }

        private static void OnDisconnect(HubConnection connection)
        {
            Console.WriteLine("Disconnected:");

            connection.StopAsync();
            connection.DisposeAsync();
            return;
        }
    }
}