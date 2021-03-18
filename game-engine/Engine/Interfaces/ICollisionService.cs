using System.Collections.Generic;
using Domain.Models;

namespace Engine.Interfaces
{
    public interface ICollisionService
    {
        int GetConsumedSizeFromPlayer(GameObject consumer, GameObject consumee);
        List<GameObject> GetCollisions(BotObject bot);
    }
}