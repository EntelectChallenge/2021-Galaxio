using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;
using Microsoft.Extensions.Options;

namespace Engine.Handlers.Collisions
{
    public class GasCloudCollisionHandler : ICollisionHandler
    {
        private readonly EngineConfig engineConfig;
        private readonly IWorldStateService worldStateService;

        public GasCloudCollisionHandler(IWorldStateService worldStateService, IConfigurationService engineConfigOptions)
        {
            this.worldStateService = worldStateService;
            engineConfig = engineConfigOptions.Value;
        }

        public bool IsApplicable(GameObject gameObject, BotObject bot) => gameObject.GameObjectType == GameObjectType.GasCloud;

        public bool ResolveCollision(GameObject gameObject, BotObject bot)
        {
            var currentEffect = new ActiveEffect
            {
                Bot = bot,
                Effect = Effects.GasCloud
            };

            /* If the effect is not registered, add it to the list. */
            if (worldStateService.GetActiveEffectByType(bot.Id, Effects.GasCloud) != default)
            {
                if (bot.Size < engineConfig.MinimumPlayerSize)
                {
                    worldStateService.RemoveGameObjectById(bot.Id);
                }
                return bot.Size >= engineConfig.MinimumPlayerSize;
            }

            worldStateService.AddActiveEffect(currentEffect);
            bot.Size -= engineConfig.GasClouds.AffectPerTick;
            if (bot.Size < engineConfig.MinimumPlayerSize)
            {
                worldStateService.RemoveGameObjectById(bot.Id);
            }
            return bot.Size >= engineConfig.MinimumPlayerSize;
        }
    }
}