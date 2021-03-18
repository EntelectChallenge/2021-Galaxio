using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Actions
{
    public class ForwardActionHandler : IActionHandler
    {
        public bool IsApplicable(PlayerAction botCurrentAction) => botCurrentAction?.Action == PlayerActions.Forward;

        public void ProcessAction(BotObject bot)
        {
            bot.CurrentHeading = bot.CurrentAction.Heading;
            bot.IsMoving = true;
        }
    }
}