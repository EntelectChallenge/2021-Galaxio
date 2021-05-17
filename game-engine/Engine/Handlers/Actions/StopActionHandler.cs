using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Actions
{
    public class StopActionHandler : IActionHandler
    {
        public bool IsApplicable(PlayerAction action) => action.Action == PlayerActions.Stop;

        public void ProcessAction(BotObject bot)
        {
            bot.IsMoving = false;
        }
    }
}