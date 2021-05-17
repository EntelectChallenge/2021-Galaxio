using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;

namespace Engine.Handlers.Actions
{
    public class StopAfterburnerActionHandler : IActionHandler
    {
        private readonly IWorldStateService worldStateService;

        public StopAfterburnerActionHandler(IWorldStateService worldStateService)
        {
            this.worldStateService = worldStateService;
        }

        public bool IsApplicable(PlayerAction action) => action?.Action == PlayerActions.StopAfterburner;

        public void ProcessAction(BotObject bot)
        {
            var activeEffect = worldStateService.GetActiveEffectByType(bot.Id, Effects.Afterburner);

            /* If the effect is not registered, remove speed boost and remove it from the list. */
            if (activeEffect != default)
            {
                worldStateService.RemoveActiveEffect(activeEffect);
                worldStateService.UpdateBotSpeed(bot);
            }
        }
    }
}