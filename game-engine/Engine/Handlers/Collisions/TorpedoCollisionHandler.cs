﻿using System;
using Domain.Enums;
using Domain.Models;
using Engine.Handlers.Interfaces;
using Engine.Interfaces;
using Engine.Models;
using Engine.Services;

namespace Engine.Handlers.Collisions
{
    public class TorpedoCollisionHandler : ICollisionHandler
    {
        private readonly EngineConfig engineConfig;
        private readonly IWorldStateService worldStateService;

        public TorpedoCollisionHandler(IConfigurationService configurationService, IWorldStateService worldStateService)
        {
            engineConfig = configurationService.Value;
            this.worldStateService = worldStateService;
        }

        public bool IsApplicable(GameObject gameObject, MovableGameObject mover) =>
            gameObject.GameObjectType == GameObjectType.TorpedoSalvo;

        public bool ResolveCollision(GameObject go, MovableGameObject mover)
        {
            if (!(go is TorpedoGameObject torpedo))
            {
                return false;
            }

            var moverStartingSize = mover.Size;
            var destructiveSize = go.Size;
            mover.Size -= go.Size;
            go.Size -= Math.Max(moverStartingSize, go.Size);
            if (go.Size <= 0)
            {
                go.Size = 0;
                worldStateService.RemoveGameObjectById(go.Id);
            }

            if (mover.Size <= 0)
            {
                mover.Size = 0;
                worldStateService.RemoveGameObjectById(mover.Id);
            }

            var firingPlayer = worldStateService.GetBotById(torpedo.FiringPlayerId);

            if (firingPlayer != null)
            {
                firingPlayer.Size += destructiveSize;
            }

            worldStateService.UpdateBotSpeed(firingPlayer);
            if (mover is BotObject bot)
            {
                worldStateService.UpdateBotSpeed(bot);
            }

            return mover.Size >= engineConfig.MinimumPlayerSize;
        }
    }
}