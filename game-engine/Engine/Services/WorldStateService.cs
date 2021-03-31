using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Engine.Interfaces;
using Engine.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Engine.Services
{
    public class WorldStateService : IWorldStateService
    {
        private readonly EngineConfig engineConfig;
        private readonly IVectorCalculatorService vectorCalculatorService;
        private readonly IWorldObjectGenerationService worldObjectGenerationService;
        private readonly List<Guid> markedForRemoval = new List<Guid>();
        private List<BotObject> lostBots = new List<BotObject>();

        private readonly GameState state = new GameState
        {
            World = null,
            GameObjects = new List<GameObject>(),
            PlayerGameObjects = new List<BotObject>()
        };

        private readonly List<ActiveEffect> activeEffects = new List<ActiveEffect>();

        private readonly GameStateDto publishedState = new GameStateDto
        {
            World = null,
            GameObjects = new Dictionary<string, List<int>>(),
            PlayerObjects = new Dictionary<string, List<int>>()
        };

        private int wormholeSeed;
        private int gasCloudSeed;
        private int asteroidFieldSeed;

        public WorldStateService(
            IConfigurationService engineConfigOptions,
            IVectorCalculatorService vectorCalculatorService,
            IWorldObjectGenerationService worldObjectGenerationService)
        {
            engineConfig = engineConfigOptions.Value;
            this.vectorCalculatorService = vectorCalculatorService;
            this.worldObjectGenerationService = worldObjectGenerationService;
            state.World = new World
            {
                Radius = engineConfig.MapRadius,
                CenterPoint = new Position(),
                CurrentTick = 0
            };
        }

        public GameObject GetObjectById(Guid objectId)
        {
            return state.GameObjects.Find(c => c.Id == objectId && !markedForRemoval.Contains(objectId));
        }

        public BotObject GetBotById(Guid botId)
        {
            return state.PlayerGameObjects.Find(c => c.Id == botId && !markedForRemoval.Contains(botId));
        }

        public void AddBotObject(BotObject bot)
        {
            state.PlayerGameObjects.Add(bot);
        }

        public GameState GetState() => state;

        public ActiveEffect GetActiveEffectByType(Guid botId, Effects effect)
        {
            return activeEffects.Find(a => a.Bot.Id == botId && a.Effect == effect);
        }

        public void AddActiveEffect(ActiveEffect activeEffect)
        {
            if (activeEffects.Contains(activeEffect))
            {
                return;
            }

            AddBotEffects(activeEffect);
            activeEffects.Add(activeEffect);
        }

        public void RemoveActiveEffect(ActiveEffect activeEffect)
        {
            if (!activeEffects.Contains(activeEffect))
            {
                return;
            }

            RemoveBotEffects(activeEffect);
            activeEffects.Remove(activeEffect);
        }

        public void AddGameObject(GameObject gameObject)
        {
            state.GameObjects.Add(gameObject);
        }

        public void RemoveGameObjectById(Guid id)
        {
            if (!markedForRemoval.Contains(id))
            {
                markedForRemoval.Add(id);
            }
        }

        public void ApplyAfterTickStateChanges()
        {
            state.World.CurrentTick++;
            state.World.Radius--;

            foreach (var statePlayerObject in state.PlayerGameObjects.Where(
                statePlayerObject => !vectorCalculatorService.IsInWorldBoundsWithOffset(
                    statePlayerObject.Position,
                    statePlayerObject.Size,
                    state.World.Radius)))
            {
                statePlayerObject.Size -= 1;
                UpdateBotSpeed(statePlayerObject);

                if (statePlayerObject.Size < 5)
                {
                    Logger.LogInfo("BotDeath", "Bot shrunk too small");
                    markedForRemoval.Add(statePlayerObject.Id);
                }
            }

            foreach (var stateGameObject in state.GameObjects.Where(
                stateGameObject => !vectorCalculatorService.IsInWorldBounds(stateGameObject.Position, state.World.Radius)))
            {
                markedForRemoval.Add(stateGameObject.Id);
            }

            foreach (var (item1, item2) in state.WormholePairs)
            {
                if (!vectorCalculatorService.IsInWorldBoundsWithOffset(item1.Position, item1.Size, state.World.Radius) ||
                    !vectorCalculatorService.IsInWorldBoundsWithOffset(item2.Position, item2.Size, state.World.Radius))
                {
                    markedForRemoval.Add(item1.Id);
                    markedForRemoval.Add(item2.Id);
                }
                else
                {
                    var newSize = (int) Math.Ceiling(item1.Size * engineConfig.Wormholes.GrowthRate);
                    item1.Size = newSize > engineConfig.Wormholes.MaxSize ? engineConfig.Wormholes.MaxSize : newSize;
                    item2.Size = item1.Size;
                }
            }

            HandeEffects();

            foreach (var gameObject in markedForRemoval.Select(guid => state.GameObjects.Find(go => go.Id == guid))
                .Where(gameObject => gameObject != default))
            {
                state.GameObjects.Remove(gameObject);

                if (gameObject.GameObjectType == GameObjectType.Wormhole)
                {
                    state.WormholePairs.Remove(GetWormholePair(gameObject.Id));
                }
            }

            foreach (var botObject in markedForRemoval.Select(guid => state.PlayerGameObjects.Find(go => go.Id == guid))
                .Where(gameObject => gameObject != default))
            {
                if (state.PlayerGameObjects.Count > 1)
                {
                    state.PlayerGameObjects.Remove(botObject);
                }

                botObject.Placement = state.PlayerGameObjects.Count + 1;
                lostBots.Add(botObject);
            }

            markedForRemoval.Clear();
        }

        public int GetPlayerCount()
        {
            return state.PlayerGameObjects.Count(go => !markedForRemoval.Contains(go.Id));
        }

        public IList<GameObject> GetCurrentGameObjects()
        {
            var allObjects = new List<GameObject>();
            allObjects.AddRange(state.GameObjects);
            allObjects.AddRange(state.PlayerGameObjects);
            return allObjects.Where(go => !markedForRemoval.Contains(go.Id)).ToList();
        }

        public bool GameObjectIsInWorldState(Guid id)
        {
            var objects = new List<GameObject>(state.PlayerGameObjects);
            objects.AddRange(state.GameObjects);
            return !markedForRemoval.Contains(id) && objects.Exists(o => o.Id == id);
        }

        public GameStateDto GetPublishedState()
        {
            publishedState.World = state.World;
            publishedState.GameObjects.Clear();
            publishedState.PlayerObjects.Clear();
            foreach (var stateGameObject in state.GameObjects)
            {
                var id = stateGameObject.Id.ToString();
                if (publishedState.GameObjects.ContainsKey(id))
                {
                    publishedState.GameObjects[id] = stateGameObject.ToStateList();
                }
                else
                {
                    publishedState.GameObjects.Add(id, stateGameObject.ToStateList());
                }
            }

            foreach (var stateGameObject in state.PlayerGameObjects)
            {
                var id = stateGameObject.Id.ToString();
                if (publishedState.GameObjects.ContainsKey(id))
                {
                    publishedState.PlayerObjects[id] = stateGameObject.ToStateList();
                }
                else
                {
                    publishedState.PlayerObjects.Add(id, stateGameObject.ToStateList());
                }
            }

            return publishedState;
        }

        public BotObject CreateBotObject(Guid id)
        {
            var bot = CreatePlayerObject(id);
            bot.Seed = engineConfig.Seeds.PlayerSeeds[state.PlayerGameObjects.Count];
            state.PlayerGameObjects.Add(bot);
            return bot;
        }

        public IList<BotObject> GetPlayerBots()
        {
            return state.PlayerGameObjects.Where(go => !markedForRemoval.Contains(go.Id)).ToList();
        }

        public void GenerateStartingWorld()
        {
            Logger.LogDebug("WorldGen", "Using the following config values");
            Logger.LogData(JsonConvert.SerializeObject(engineConfig,Formatting.Indented));
            List<int> playerSeeds = engineConfig.Seeds.PlayerSeeds ??
                worldObjectGenerationService.GeneratePlayerSeeds(state.PlayerGameObjects);
            engineConfig.Seeds.PlayerSeeds = playerSeeds;
            Logger.LogDebug("WorldGen", $"Player Seeds: [{string.Join(" ", playerSeeds)}]");
            List<GameObject> placedFood = worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, state.GameObjects);
            Logger.LogDebug("WorldGen", $"Placed Starting Food, GameObject Count: {state.GameObjects.Count}");
            wormholeSeed = engineConfig.Wormholes.Seed ?? new Random().Next(engineConfig.Wormholes.MinSeed, engineConfig.Wormholes.MaxSeed);
            Logger.LogDebug("WorldGen", $"Wormhole Seed: {wormholeSeed}");
            state.WormholePairs = worldObjectGenerationService.GenerateWormholes(state.GameObjects, wormholeSeed);
            Logger.LogDebug("WorldGen", $"Placed Wormholes, GameObject Count: {state.GameObjects.Count}");

            /* Get a new seed if one was not specified. */
            gasCloudSeed = engineConfig.GasClouds.Seed ?? new Random().Next(engineConfig.GasClouds.MinSeed, engineConfig.GasClouds.MaxSeed);
            Logger.LogDebug("WorldGen", $"First Gas Cloud Seed: {gasCloudSeed}");

            /* Generate the gasClouds and set the state values. */
            Tuple<List<GameObject>, int> gasClouds = worldObjectGenerationService.GenerateGasClouds(state.GameObjects, gasCloudSeed);
            state.GasClouds = gasClouds.Item1;
            gasCloudSeed = gasClouds.Item2;
            Logger.LogDebug("WorldGen", $"Final Gas Cloud Seed: {gasCloudSeed}");
            Logger.LogDebug("WorldGen", $"Placed Gas Clouds: {state.GameObjects.Count}");

            /* Get a new seed if one was not specified. */
            asteroidFieldSeed = engineConfig.AsteroidFields.Seed ??
                new Random().Next(engineConfig.AsteroidFields.MinSeed, engineConfig.AsteroidFields.MaxSeed);
            Logger.LogDebug("WorldGen", $"First Asteroid Field Seed: {asteroidFieldSeed}");

            /* Generate the asteroidFields and set the state values. */
            Tuple<List<GameObject>, int> asteroidFields =
                worldObjectGenerationService.GenerateAsteroidFields(state.GameObjects, asteroidFieldSeed);
            state.AsteroidFields = asteroidFields.Item1;
            asteroidFieldSeed = asteroidFields.Item2;
            Logger.LogDebug("WorldGen", $"Final Asteroid Field Seed: {asteroidFieldSeed}");
            Logger.LogDebug("WorldGen", $"Placed Asteroid Fields: {state.GameObjects.Count}");

            worldObjectGenerationService.GenerateWorldFood(placedFood, playerSeeds, state.GameObjects);
            Logger.LogDebug("WorldGen", $"Placed Food, GameObject Count: {state.GameObjects.Count}");
        }

        public void UpdateBotSpeed(GameObject bot)
        {
            if (bot.Size == 0)
            {
                bot.Speed = 0;
                return;
            }

            bot.Speed = (int) Math.Ceiling(engineConfig.Speeds.Ratio / bot.Size);

            if (GetActiveEffectByType(bot.Id, Effects.Afterburner) != null)
            {
                bot.Speed *= engineConfig.Afterburners.SpeedFactor;
            }

            if (GetActiveEffectByType(bot.Id, Effects.AsteroidField) != null && bot.Speed > 1)
            {
                bot.Speed -= engineConfig.AsteroidFields.AffectPerTick;
            }
        }

        public Tuple<GameObject, GameObject> GetWormholePair(Guid gameObjectId)
        {
            return state.WormholePairs.Find(pair => pair.Item1.Id == gameObjectId || pair.Item2.Id == gameObjectId);
        }

        public void UpdateGameObject(GameObject gameObject)
        {
            // TODO Might cause concurrent modification exceptions. Test thoroughly.
            var index = state.GameObjects.FindIndex(go => go.Id == gameObject.Id);
            state.GameObjects[index] = gameObject;
        }

        public GameCompletePayload GenerateGameCompletePayload()
        {
            try
            {
                return new GameCompletePayload
                {
                    TotalTicks = state.World.CurrentTick,
                    WinningBot = lostBots.First(),
                    WorldSeeds = new List<int>
                    {
                        wormholeSeed
                    },
                    Players = GeneratePlayerResults()
                };
            }
            catch (InvalidOperationException)
            {
                Logger.LogError("GameComplete", "One or more seeds were null at the end of game");
                throw;
            }
        }

        public List<BotObject> FinalisePlayerPlacements()
        {
            var finalBotList = new List<BotObject>();
            finalBotList.AddRange(lostBots);
            finalBotList.AddRange(state.PlayerGameObjects);

            finalBotList = finalBotList.OrderByDescending(bot => bot.Size).ThenByDescending(bot => bot.Score).ToList();

            foreach (var botOfInterest in finalBotList.Where(botOfInterest => botOfInterest.Placement == default))
            {
                botOfInterest.Placement = state.PlayerGameObjects.IndexOf(botOfInterest) + 1;
            }

            lostBots = finalBotList;

            return finalBotList;
        }

        private void AddBotEffects(ActiveEffect activeEffect)
        {
            var bot = state.PlayerGameObjects.Find(p => p.Id == activeEffect.Bot.Id);

            if (bot != default)
            {
                bot.Effects |= activeEffect.Effect;
            }
        }

        private void RemoveBotEffects(ActiveEffect activeEffect)
        {
            var bot = state.PlayerGameObjects.Find(p => p.Id == activeEffect.Bot.Id);

            if (bot != default)
            {
                bot.Effects &= activeEffect.Effect;
            }
        }

        private List<PlayerResult> GeneratePlayerResults() =>
            lostBots.Select(
                    bot => new PlayerResult
                    {
                        Id = bot.Id.ToString(),
                        Placement = bot.Placement,
                        MatchPoints = GetMatchPointsFromPlacement(bot.Placement),
                        Score = bot.Score,
                        Seed = bot.Seed != default ? bot.Seed : throw new InvalidOperationException("Null Player Seed")
                    })
                .ToList();

        private int GetMatchPointsFromPlacement(int placement) => (engineConfig.BotCount - placement + 1) * 2;

        private BotObject CreatePlayerObject(Guid id) =>
            new BotObject
            {
                Id = id,
                Size = engineConfig.StartingPlayerSize,
                Position = vectorCalculatorService.GetNewPlayerStartingPosition(
                    GetPlayerCount(),
                    engineConfig.BotCount,
                    engineConfig.StartRadius),
                Speed = engineConfig.Speeds.StartingSpeed,
                GameObjectType = GameObjectType.Player,
                PendingActions = new List<PlayerAction>(),
                CurrentAction = new PlayerAction
                {
                    Action = PlayerActions.Stop,
                    Heading = 0,
                    PlayerId = id
                },
                Score = 0
            };

        private void HandeEffects()
        {
            var effectsToRemove = new List<ActiveEffect>();

            /* Handle Afterburner costs. */
            foreach (var activeEffect in activeEffects.Where(a => a.Effect == Effects.Afterburner))
            {
                /* Only reduce bot size if it has an active Afterburner. */
                var bot = state.PlayerGameObjects.Find(b => b.Id == activeEffect.Bot.Id);

                if (bot != default)
                {
                    bot.Size -= engineConfig.Afterburners.SizeConsumptionPerTick;
                }
            }

            /* Handle Gas Cloud effects. */
            foreach (var activeEffect in activeEffects.Where(a => a.Effect == Effects.GasCloud))
            {
                /* Only reduce bot size if it is in a Gas Cloud. */
                var bot = state.PlayerGameObjects.Find(b => b.Id == activeEffect.Bot.Id);

                if (bot == default)
                {
                    continue;
                }

                if (GetCollisions(bot).All(c => c.GameObjectType != GameObjectType.GasCloud))
                {
                    // This is state in place modification
                    effectsToRemove.Add(activeEffect);
                }
                else
                {
                    bot.Size -= engineConfig.GasClouds.AffectPerTick;
                }
            }

            /* Handle Asteroid Field effects. */
            foreach (var activeEffect in activeEffects.Where(a => a.Effect == Effects.AsteroidField))
            {
                /* Only reduce bot speed if it is in an Asteroid Field. */
                var bot = state.PlayerGameObjects.Find(b => b.Id == activeEffect.Bot.Id);

                if (bot == default)
                {
                    continue;
                }

                if (GetCollisions(bot).All(c => c.GameObjectType != GameObjectType.AsteroidField))
                {
                    effectsToRemove.Add(activeEffect);
                }
                else
                {
                    UpdateBotSpeed(bot);
                }
            }

            effectsToRemove.ForEach(RemoveActiveEffect);
        }

        /* I need to have this GetCollisions in the WorldStateService in order to check for collisions with Gas Clouds and Asteroid Fields every tick. */
        private List<GameObject> GetCollisions(BotObject bot)
        {
            IList<GameObject> gameObjects = GetCurrentGameObjects();
            return gameObjects.Where(go => go.Id != bot.Id && vectorCalculatorService.HasOverlap(go, bot)).ToList();
        }
    }
}