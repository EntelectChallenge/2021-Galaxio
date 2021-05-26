using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

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

        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) => gameObject.GameObjectType == GameObjectType.GasCloud;

        public bool ResolveCollision(GameObject go, MovableGameObject mover)
        {
            var currentEffect = new ActiveEffect
            {
                Bot = mover,
                Effect = Effects.GasCloud
            };

            if (mover is BotObject bot)
            {
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

            var moverStartingSize = mover.Size;
            mover.Size -= go.Size;
            go.Size -= moverStartingSize;
            if (go.Size <= 0)
            {
                go.Size = 0;
                worldStateService.RemoveGameObjectById(go.Id);
            }

            if (mover.Size <= 0)
            {
                mover.Size = 0;
                worldStateService.RemoveGameObjectById(mover.Id);
            }

            return mover.Size > 0;
        }
    }
}