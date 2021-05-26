using System;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;

namespace Engine.Services
{
    public class ActionService : IActionService
    {
        private readonly IWorldStateService worldStateService;
        private readonly IActionHandlerResolver actionHandlerResolver;

        public ActionService(IWorldStateService worldStateService, IActionHandlerResolver actionHandlerResolver)
        {
            this.worldStateService = worldStateService;
            this.actionHandlerResolver = actionHandlerResolver;
        }

        public void PushPlayerAction(Guid botId, PlayerAction playerAction)
        {
            var targetBot = worldStateService.GetBotById(playerAction.PlayerId);
            targetBot?.PendingActions.Add(playerAction);
        }

        public void ApplyActionToBot(BotObject bot)
        {
            if (bot.PendingActions != null &&
                bot.PendingActions.Count > 0)
            {
                bot.CurrentAction = bot.PendingActions[0];
                bot.PendingActions.RemoveAt(0);
            }

            if (bot.CurrentAction == default)
            {
                return;
            }

            var handler = actionHandlerResolver.ResolveHandler(bot.CurrentAction);
            handler.ProcessAction(bot);
            bot.LastAction = bot.CurrentAction;
        }
    }
}