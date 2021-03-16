using Domain.Models;

namespace Engine.Handlers.Interfaces
{
    public interface IActionHandlerResolver
    {
        IActionHandler ResolveHandler(PlayerAction botCurrentAction);
    }
}