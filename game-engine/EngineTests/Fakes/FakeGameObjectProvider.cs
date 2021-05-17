using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Models;
using Engine.Interfaces;

namespace EngineTests.Fakes
{
    public class FakeGameObjectProvider
    {
        private readonly IWorldStateService worldStateService;
        private readonly IWorldObjectGenerationService worldObjectGenerationService;

        public FakeGameObjectProvider(IWorldStateService worldStateService, IWorldObjectGenerationService worldObjectGenerationService)
        {
            this.worldStateService = worldStateService;
            this.worldObjectGenerationService = worldObjectGenerationService;
        }

        public List<Tuple<GameObject, GameObject>> GetWormholes(int seed)
        {
            var gameObjects = new List<GameObject>();
            List<Tuple<GameObject, GameObject>> wormholes = worldObjectGenerationService.GenerateWormholes(gameObjects, seed);

            return wormholes;
        }

        public List<GameObject> GetGasClouds()
        {
            var gameObjects = new List<GameObject>();
            Tuple<List<GameObject>, int> gasClouds = worldObjectGenerationService.GenerateGasClouds(gameObjects, 12345);

            return gasClouds.Item1;
        }

        public List<GameObject> GetAsteroidFields()
        {
            var gameObjects = new List<GameObject>();
            Tuple<List<GameObject>, int> asteroidFields = worldObjectGenerationService.GenerateAsteroidFields(gameObjects, 54321);

            return asteroidFields.Item1;
        }

        public GameObject GetFoodAt(Position position)
        {
            var bot = new GameObject
            {
                Id = Guid.NewGuid(),
                Size = 1,
                Position = position,
                Speed = 0,
                GameObjectType = GameObjectType.Food
            };
            worldStateService.AddGameObject(bot);
            return bot;
        }
        
        public GameObject GetSuperfoodAt(Position position)
        {
            var superfood = new GameObject
            {
                Id = Guid.NewGuid(),
                Size = 1,
                Position = position,
                Speed = 0,
                GameObjectType = GameObjectType.Superfood
            };
            worldStateService.AddGameObject(superfood);
            return superfood;
        }

        public GameObject GetSmallBotAt(Position position)
        {
            var bot = new BotObject
            {
                Id = Guid.NewGuid(),
                Size = 5,
                Position = position,
                Speed = 40,
                GameObjectType = GameObjectType.Player
            };
            worldStateService.AddBotObject(bot);
            return bot;
        }

        public BotObject GetBigBotAt(Position position)
        {
            var id = Guid.NewGuid();
            var bot = new BotObject
            {
                Id = id,
                Size = 20,
                Position = position,
                Speed = 10,
                GameObjectType = GameObjectType.Player,
                CurrentAction = new PlayerAction
                {
                    Action = PlayerActions.Stop,
                    Heading = 0,
                    PlayerId = id
                }
            };
            worldStateService.AddBotObject(bot);
            return bot;
        }

        public BotObject GetBotAt(Position position)
        {
            var id = Guid.NewGuid();
            var bot = new BotObject
            {
                Id = id,
                Size = 10,
                Position = position,
                Speed = 20,
                GameObjectType = GameObjectType.Player,
                CurrentAction = new PlayerAction
                {
                    Action = PlayerActions.Stop,
                    Heading = 0,
                    PlayerId = id
                }
            };
            worldStateService.AddBotObject(bot);
            return bot;
        }

        public BotObject GetBotAtDefault()
        {
            var bot = new BotObject
            {
                Id = Guid.NewGuid(),
                Size = 10,
                Position = new Position(),
                Speed = 20,
                GameObjectType = GameObjectType.Player
            };
            worldStateService.AddBotObject(bot);
            return bot;
        }

        public BotObject GetBotWithActions()
        {
            var id = Guid.NewGuid();
            var bot = new BotObject
            {
                Id = id,
                Size = 10,
                Position = new Position(),
                Speed = 20,
                GameObjectType = GameObjectType.Player,
                PendingActions = new List<PlayerAction>
                {
                    GetForwardPlayerAction(id),
                    GetForwardPlayerAction(id)
                },
                CurrentAction = new PlayerAction
                {
                    Action = PlayerActions.Stop,
                    Heading = 0,
                    PlayerId = id
                }
            };
            worldStateService.AddBotObject(bot);
            return bot;
        }

        public PlayerAction GetForwardPlayerAction(Guid botId) =>
            new PlayerAction
            {
                Action = PlayerActions.Forward,
                Heading = 0,
                PlayerId = botId
            };

        public PlayerAction GetForwardPlayerActionInHeading(Guid botId, int heading) =>
            new PlayerAction
            {
                Action = PlayerActions.Forward,
                Heading = heading,
                PlayerId = botId
            };

        public PlayerAction GetStartAfterburnerPlayerAction(Guid botId) =>
            new PlayerAction
            {
                Action = PlayerActions.StartAfterburner,
                PlayerId = botId
            };

        public PlayerAction GetStopAfterburnerPlayerAction(Guid botId) =>
            new PlayerAction
            {
                Action = PlayerActions.StopAfterburner,
                PlayerId = botId
            };
    }
}