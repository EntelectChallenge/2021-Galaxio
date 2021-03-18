using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;

namespace Engine.Interfaces
{
    public interface IWorldStateService
    {
        GameObject GetObjectById(Guid objectId);
        GameState GetState();
        ActiveEffect GetActiveEffectByType(Guid botId, Effects effect);
        void AddActiveEffect(ActiveEffect activeEffect);
        void RemoveActiveEffect(ActiveEffect activeEffect);
        void AddGameObject(GameObject gameObject);
        void RemoveGameObjectById(Guid id);
        IList<BotObject> GetPlayerBots();
        void GenerateStartingWorld();
        void ApplyAfterTickStateChanges();
        int GetPlayerCount();
        IList<GameObject> GetCurrentGameObjects();
        bool GameObjectIsInWorldState(Guid id);
        GameStateDto GetPublishedState();
        BotObject CreateBotObject(Guid id);
        BotObject GetBotById(Guid playerActionPlayerId);
        void AddBotObject(BotObject bot);
        void UpdateBotSpeed(GameObject bot);
        Tuple<GameObject, GameObject> GetWormholePair(Guid gameObjectId);
        void UpdateGameObject(GameObject gameObject);
        GameCompletePayload GenerateGameCompletePayload();
        List<BotObject> FinalisePlayerPlacements();
    }
}