using System.Linq;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;

namespace Engine.Handlers.Actions
{
    public class TeleportActionHandler: IActionHandler
    {
        private readonly IWorldStateService worldStateService;

        public TeleportActionHandler(IWorldStateService worldStateService)
        {
            this.worldStateService = worldStateService;
        }

        public bool IsApplicable(PlayerAction action) => action.Action == PlayerActions.Teleport;

        public void ProcessAction(BotObject bot)
        {
            var teleporter = worldStateService.GetActiveTeleporterForBot(bot.Id);
            if (teleporter is null)
            {
                return;
            }

            worldStateService.TeleportBot(teleporter, bot);
        }
    }
}