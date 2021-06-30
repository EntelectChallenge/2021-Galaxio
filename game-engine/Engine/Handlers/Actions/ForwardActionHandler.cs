using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Actions
{
    public class ForwardActionHandler : IActionHandler
    {
        public bool IsApplicable(PlayerAction action) => action?.Action == PlayerActions.Forward;

        public void ProcessAction(BotObject bot)
        {
            bot.CurrentHeading = bot.CurrentAction.Heading;
            bot.ShouldCalculateCollisionPaths = true;
        }
    }
}