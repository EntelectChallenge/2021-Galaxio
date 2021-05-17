using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;
using Microsoft.Extensions.Options;

namespace Engine.Handlers.Actions
{
    public class StartAfterburnerActionHandler : IActionHandler
    {
        private readonly IWorldStateService worldStateService;
        private readonly EngineConfig engineConfig;

        public StartAfterburnerActionHandler(IWorldStateService worldStateService, IConfigurationService engineConfig)
        {
            this.worldStateService = worldStateService;
            this.engineConfig = engineConfig.Value;
        }

        public bool IsApplicable(PlayerAction action) => action?.Action == PlayerActions.StartAfterburner;

        public void ProcessAction(BotObject bot)
        {
            /* Bot does not have enough resources to consume for the afterburner. */
            if (bot.Size <= engineConfig.Afterburners.SizeConsumptionPerTick ||
                bot.Size <= engineConfig.MinimumPlayerSize)
            {
                return;
            }

            var currentEffect = new ActiveEffect
            {
                Bot = bot,
                Effect = Effects.Afterburner
            };

            /* If the effect is not registered, add speed boost and add it to the list. */
            if (worldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner) == default)
            {
                worldStateService.AddActiveEffect(currentEffect);
                worldStateService.UpdateBotSpeed(bot);
            }
        }
    }
}