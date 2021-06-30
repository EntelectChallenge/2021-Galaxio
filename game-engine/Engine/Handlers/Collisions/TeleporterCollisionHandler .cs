using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Collisions
{
    public class TeleporterCollisionHandler : ICollisionHandler
    {
        private readonly EngineConfig engineConfig;
        private readonly IWorldStateService worldStateService;

        public TeleporterCollisionHandler(IConfigurationService configurationService, IWorldStateService worldStateService)
        {
            engineConfig = configurationService.Value;
            this.worldStateService = worldStateService;
        }

        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) =>
            gameObject.GameObjectType == GameObjectType.Teleporter;

        public bool ResolveCollision(GameObject go, MovableGameObject mover) => true; // no-op. Nothing collides with the bomb
    }
}