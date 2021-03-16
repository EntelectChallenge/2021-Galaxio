using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;
using Microsoft.Extensions.Options;

namespace Engine.Handlers.Collisions
{
    public class FoodCollisionHandler : ICollisionHandler
    {
        private readonly IWorldStateService worldStateService;
        private readonly EngineConfig engineConfig;

        public FoodCollisionHandler(IWorldStateService worldStateService, IConfigurationService engineConfigOptions)
        {
            this.worldStateService = worldStateService;
            engineConfig = engineConfigOptions.Value;
        }

        public bool IsApplicable(GameObject gameObject, BotObject bot) => gameObject.GameObjectType == GameObjectType.Food;

        public bool ResolveCollision(GameObject go, BotObject bot)
        {
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

            if (bot.Size > engineConfig.WorldFood.MaxConsumptionSize)
            {
                return true;
            }

            bot.Size += go.Size;
            bot.Score += engineConfig.ScoreRates[GameObjectType.Food];
            go.Size = 0;
            worldStateService.RemoveGameObjectById(go.Id);
            worldStateService.UpdateBotSpeed(bot);
            return true;
        }
    }
}