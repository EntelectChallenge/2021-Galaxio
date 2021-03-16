using Microsoft.AspNetCore.SignalR.Client;
using NETCoreBot.Enums;
using NETCoreBot.Models;
using NETCoreBot.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NETCoreBot
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            /* TODO: Replace this url with config. */
            var url = "https://localhost:44390/runnerhub";

            var connection = new HubConnectionBuilder()
                .WithUrl($"{url}")
                .WithAutomaticReconnect()
                .Build();

            var botService = new BotService();

            await connection.StartAsync().ContinueWith(task =>
            {
                /* Clients should disconnect from the server when the server sends the request to do so. */
                connection.On<Guid>("Disconnect", (Id) =>
                {
                    Console.WriteLine("Disconnected:");

                    connection.StopAsync();
                });

                /* Get the current WorldState along with the last known state of the current client. */
                connection.On<GameState, GameObject>("ReceiveGameState", (gameState, bot) =>
                {
                    Console.WriteLine("Current ConnectionId: " + bot.Id);

                    botService.SetBot(bot);
                    botService.SetGameState(gameState);

                    foreach (var _bot in gameState.GameObjects.Where(o => o.ObjectType == ObjectTypes.Player))
                    {
                        Console.WriteLine(String.Format("Id - {0}, PositionX - {1}, PositionY - {2}, Size - {3}", _bot.Id, _bot.Position.X, _bot.Position.Y, _bot.Size));
                    }
                });

                while (true)
                {
                    /* This sleep is important to sync the two calls below. */
                    Thread.Sleep(1000);

                    var bot = botService.GetBot();

                    if (bot == null)
                        return;

                    connection.InvokeAsync("RequestGameState");

                    /* TODO: Add bot logic here between RequestWorldState and SendClientAction, use SetClient to form object to be sent to Runner. */
                    botService.SetPlayerAction(botService.GetPlayerAction());

                    connection.InvokeAsync("SendPlayerAction", botService.GetPlayerAction());
                }
            });
        }
    }
}
