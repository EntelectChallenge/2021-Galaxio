using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;
using Microsoft.Extensions.Options;

namespace Engine.Handlers.Collisions
{
    public class PlayerCollisionHandler : ICollisionHandler
    {
        private readonly EngineConfig engineConfig;
        private readonly IWorldStateService worldStateService;
        private readonly ICollisionService collisionService;
        private readonly IVectorCalculatorService vectorCalculatorService;

        public PlayerCollisionHandler(
            IWorldStateService worldStateService,
            ICollisionService collisionService,
            IConfigurationService engineConfigOptions,
            IVectorCalculatorService vectorCalculatorService)
        {
            engineConfig = engineConfigOptions.Value;
            this.worldStateService = worldStateService;
            this.collisionService = collisionService;
            this.vectorCalculatorService = vectorCalculatorService;
        }

        public bool IsApplicable(GameObject gameObject, BotObject bot) => gameObject.GameObjectType == GameObjectType.Player;

        public bool ResolveCollision(GameObject gameObject, BotObject bot)
        {
            if (!(gameObject is BotObject go))
            {
                throw new ArgumentException("Non player in player collision");
            }

            // If the bot's ID has already been removed from the world, the bot is dead, return the alive state as false
            if (!worldStateService.GameObjectIsInWorldState(bot.Id))
            {
                return false;
            }

            // If the colliding GO has already been removed from the world, but we reached here, the bot is alive but need not process the GO collision
            if (!worldStateService.GameObjectIsInWorldState(go.Id))
            {
                return true;
            }

            var botsAreEqualSize = bot.Size == go.Size;
            if (botsAreEqualSize)
            {
                BounceBots(go, bot, 1);
                return true;
            }

            var botIsBigger = bot.Size > go.Size;
            var consumer = botIsBigger ? bot : go;
            var consumee = !botIsBigger ? bot : go;

            var consumedSize = collisionService.GetConsumedSizeFromPlayer(consumer, consumee);

            consumee.Size -= consumedSize;
            consumer.Size += consumedSize;
            consumer.Score += engineConfig.ScoreRates[GameObjectType.Player];

            worldStateService.UpdateBotSpeed(consumer);

            BounceBots(consumee, consumer, (int)Math.Ceiling((consumedSize + 1d) / 2));

            if (consumee.Size < engineConfig.MinimumPlayerSize)
            {
                consumer.Size += consumee.Size; // After the previous consumptionSize has already been removed
                consumee.Size = 0;
                worldStateService.RemoveGameObjectById(consumee.Id);
            }

            worldStateService.UpdateBotSpeed(consumee);

            if (bot.Size > engineConfig.MinimumPlayerSize)
            {
                return true;
            }

            Logger.LogInfo("BotDeath", "Bot Consumed");
            worldStateService.RemoveGameObjectById(bot.Id);
            return false;
        }

        private void BounceBots(BotObject go, BotObject bot, int spacing)
        {
            var bots = new List<BotObject>
            {
                go,
                bot
            };

            foreach (var botObject in bots)
            {
                botObject.CurrentHeading = vectorCalculatorService.ReverseHeading(botObject.CurrentHeading);
                botObject.CurrentAction.Heading = botObject.CurrentHeading;
                botObject.Position = vectorCalculatorService.MovePlayerObject(botObject.Position, spacing, botObject.CurrentHeading);
            }
        }
    }
}