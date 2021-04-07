using Domain.Models;
using Engine.Handlers.Interfaces;

namespace Engine.Handlers.Actions
{
    public class NoOpActionHandler: IActionHandler
    {
        public bool IsApplicable(PlayerAction botCurrentAction) => false;

        public void ProcessAction(BotObject bot)
        {
            // no-op
        }
    }
}