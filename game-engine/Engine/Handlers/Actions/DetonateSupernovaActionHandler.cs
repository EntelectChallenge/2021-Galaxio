using System.Linq;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Services;

namespace Engine.Handlers.Actions
{
    public class DetonateSupernovaActionHandler: IActionHandler
    {
        private readonly IWorldStateService worldStateService;

        public DetonateSupernovaActionHandler(IWorldStateService worldStateService)
        {
            this.worldStateService = worldStateService;
        }

        public bool IsApplicable(PlayerAction action) => action.Action == PlayerActions.DetonateSupernova;

        public void ProcessAction(BotObject bot)
        {
            var supernova = worldStateService.GetState().GameObjects.OfType<SupernovaBombObject>().FirstOrDefault();
            if (supernova is null)
            {
                return;
            }

            if (!supernova.FiringPlayerId.Equals(bot.Id))
            {
                return;
            }

            Logger.LogDebug("SUPERNOVA", $"Supernova timed to detonate at {supernova.Position}");
            worldStateService.MarkSupernovaForDetonation(bot);
        }
    }
}