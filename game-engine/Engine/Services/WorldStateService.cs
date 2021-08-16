using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Domain.Models;
using Domain.Services;
using Engine.Interfaces;
using Engine.Models;
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
        private int supernovaSpawnTick = 0;
        private bool supernovaLaunched = false;
        private bool supernovaShouldDetonate = false;

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
            return !markedForRemoval.Contains(id) &&
                   (state.PlayerGameObjects.Exists(o => o.Id == id) || state.GameObjects.Exists(o => o.Id == id));
        }

        public GameStateDto GetPublishedState()
        {
            var stoplog = new StopWatchLogger();
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

            stoplog.Log("Got Published state");

            return publishedState;
        }

        public BotObject CreateBotObject(Guid id)
        {
            var bot = CreatePlayerObject(id);
            bot.Seed = engineConfig.Seeds.PlayerSeeds[state.PlayerGameObjects.Count];
            bot.TeleporterCount = engineConfig.Teleport.StartChargeCount;
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
            Logger.LogData(JsonConvert.SerializeObject(engineConfig, Formatting.Indented));
            List<int> playerSeeds = engineConfig.Seeds.PlayerSeeds ??
                                    worldObjectGenerationService.GeneratePlayerSeeds(state.PlayerGameObjects);
            engineConfig.Seeds.PlayerSeeds = playerSeeds;
            Logger.LogDebug("WorldGen", $"Player Seeds: [{string.Join(" ", playerSeeds)}]");
            List<GameObject> placedFood =
                worldObjectGenerationService.GeneratePlayerStartingFood(playerSeeds, state.GameObjects);
            Logger.LogDebug("WorldGen", $"Placed Starting Food, GameObject Count: {state.GameObjects.Count}");
            wormholeSeed = engineConfig.Wormholes.Seed ??
                           new Random().Next(engineConfig.Wormholes.MinSeed, engineConfig.Wormholes.MaxSeed);
            Logger.LogDebug("WorldGen", $"Wormhole Seed: {wormholeSeed}");
            state.WormholePairs = worldObjectGenerationService.GenerateWormholes(state.GameObjects, wormholeSeed);
            Logger.LogDebug("WorldGen", $"Placed Wormholes, GameObject Count: {state.GameObjects.Count}");

            /* Get a new seed if one was not specified. */
            gasCloudSeed = engineConfig.GasClouds.Seed ??
                           new Random().Next(engineConfig.GasClouds.MinSeed, engineConfig.GasClouds.MaxSeed);
            Logger.LogDebug("WorldGen", $"First Gas Cloud Seed: {gasCloudSeed}");

            /* Generate the gasClouds and set the state values. */
            Tuple<List<GameObject>, int> gasClouds =
                worldObjectGenerationService.GenerateGasClouds(state.GameObjects, gasCloudSeed);
            state.GasClouds = gasClouds.Item1;
            gasCloudSeed = gasClouds.Item2;
            Logger.LogDebug("WorldGen", $"Final Gas Cloud Seed: {gasCloudSeed}");
            Logger.LogDebug("WorldGen", $"Placed Gas Clouds: {state.GameObjects.Count}");

            /* Get a new seed if one was not specified. */
            asteroidFieldSeed = engineConfig.AsteroidFields.Seed ??
                                new Random().Next(engineConfig.AsteroidFields.MinSeed,
                                    engineConfig.AsteroidFields.MaxSeed);
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

            engineConfig.Supernova.Seed =
                worldObjectGenerationService.GenerateSupernovaSeed(engineConfig.Supernova.Seed.GetValueOrDefault());
            Logger.LogDebug("WorldGen", $"Generated Supernova Seed: {engineConfig.Supernova.Seed}");
        }

        public void UpdateBotSpeed(GameObject bot)
        {
            if (bot == null)
            {
                return;
            }

            if (bot.Size == 0)
            {
                bot.Speed = 0;
                return;
            }

            bot.Speed = (int) Math.Ceiling(engineConfig.Speeds.Ratio / bot.Size);

            if (GetActiveEffectByType(bot.Id, Effects.Afterburner) != null)
            {
                bot.Speed += engineConfig.Afterburners.SpeedFactor;
            }

            if (GetActiveEffectByType(bot.Id, Effects.AsteroidField) != null &&
                bot.Speed > 1)
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

        public List<MovableGameObject> GetMovableObjects()
        {
            var movableObjects = new List<MovableGameObject>();
            movableObjects.AddRange(GetPlayerBots());
            movableObjects.AddRange(state.GameObjects.OfType<MovableGameObject>());
            return movableObjects;
        }

        public void AddTeleport(int heading, Position position, BotObject bot)
        {
            var teleporter = new TeleporterObject()
            {
                Id = Guid.NewGuid(),
                CurrentHeading = heading,
                Position = position,
                GameObjectType = GameObjectType.Teleporter,
                Size = engineConfig.Teleport.Size,
                Speed = engineConfig.Teleport.Speed,
                FiringPlayerId = bot.Id
            };

            bot.Size -= engineConfig.Teleport.Cost;
            bot.TeleporterCount--;
            state.GameObjects.Add(teleporter);

            Logger.LogDebug("TELEPORT", "Teleport fired and is travelling " + position.X + ";" + position.Y);
        }

        public void AddSupernova(int heading, Position position, Guid botId)
        {
            Logger.LogDebug("SUPERNOVA", "Supernova bomb fired and is travelling");
            state.GameObjects.Add(new SupernovaBombObject
            {
                Id = Guid.NewGuid(),
                CurrentHeading = heading,
                Position = position,
                GameObjectType = GameObjectType.SupernovaBomb,
                Size = engineConfig.Supernova.Size,
                Speed = engineConfig.Supernova.Speed,
                FiringPlayerId = botId
            });
            supernovaLaunched = true;
        }

        public void AddTorpedo(BotObject bot)
        {
            var torpedoSalvo = new TorpedoGameObject
            {
                Id = Guid.NewGuid(),
                Position = vectorCalculatorService.GetPositionFrom(
                    bot.Position,
                    bot.Size + engineConfig.Torpedo.Size + 1,
                    bot.CurrentAction.Heading),
                Size = engineConfig.Torpedo.Size,
                Speed = engineConfig.Torpedo.Speed,
                CurrentHeading = bot.CurrentAction.Heading,
                FiringPlayerId = bot.Id,
                ShouldCalculateCollisionPaths = true
            };
            bot.Size -= engineConfig.Torpedo.Cost;
            bot.TorpedoSalvoCount--;

            UpdateBotSpeed(bot);
            AddGameObject(torpedoSalvo);
        }

        private void DetonateSupernova()
        {
            if (!supernovaLaunched)
            {
                return;
            }

            var supernova = state.GameObjects.FirstOrDefault(go => go.GameObjectType == GameObjectType.SupernovaBomb);
            if (supernova == null)
            {
                Logger.LogWarning("Supernova",
                    "Detonation called, with a launched supernova bomb but no matching supernova");
                return;
            }

            supernova.Size = (int) (engineConfig.Supernova.ExplosionSizeRatio * state.World.Radius);
            Logger.LogDebug("SUPERNOVA", $"Supernova detonated with radius {supernova.Size}");

            var botsInDamageRadius =
                state.PlayerGameObjects.Where(bot => vectorCalculatorService.HasOverlap(supernova, bot));
            foreach (var bot in botsInDamageRadius)
            {
                bot.Size -= engineConfig.Supernova.Damage;
            }

            Logger.LogDebug("SUPERNOVA",
                $"Damaged: {string.Join(',', botsInDamageRadius.Select(bot => bot.Id.ToString()))}");

            var resultingCloud = new GameObject
            {
                Id = Guid.NewGuid(),
                Position = supernova.Position,
                Effects = 0,
                Size = supernova.Size,
                Speed = 0,
                CurrentHeading = 0,
                GameObjectType = GameObjectType.GasCloud
            };

            state.GameObjects.Add(resultingCloud);
            Logger.LogDebug("SUPERNOVA",
                $"Spawned and added cloud, position: {resultingCloud.Position.ToString()}, Size: {resultingCloud.Size}");
            state.GameObjects.Remove(supernova);

            supernovaLaunched = false;
            supernovaShouldDetonate = false;
        }

        public void MarkSupernovaForDetonation(BotObject botObject)
        {
            supernovaShouldDetonate = true;
        }

        public void TeleportBot(GameObject teleporter, BotObject botObject)
        {
            Logger.LogDebug("TELEPORT", $"Player {botObject.Id} is teleporting to {teleporter.Position}");
            botObject.Position = teleporter.Position;

            markedForRemoval.Add(teleporter.Id);
        }

        public GameObject GetActiveTeleporterForBot(Guid botId)
        {
            return state.GameObjects.OfType<TeleporterObject>().FirstOrDefault(go => go.FiringPlayerId == botId);
        }

        public void ApplyAfterTickStateChanges()
        {
            ModifyWorldBaseStateForNextTick();

            UpdatePlayersExceedingWorldBounds();

            MarkExceedingWorldBoundsGameObjectsForRemoval();

            UpdateWormholeSizeAndMarkExceedingWorldBounds();

            HandleEffects();

            RemoveGameObjectsMarkedForRemoval();

            RemoveAndPlaceBotsMarkedForRemoval();

            AddTorpedoSalvos();

            AddTeleporter();

            AddShieldCount();

            ManageSupernova();

            ManageTeleporter();

            markedForRemoval.Clear();
        }

        private void ManageTeleporter()
        {
            foreach (var teleporter in state.GameObjects.Where(stateGameObject =>
                stateGameObject.GameObjectType == GameObjectType.Teleporter))
            {
                var positionUpdate = vectorCalculatorService.GetPositionFrom(teleporter.Position, teleporter.Speed,
                    teleporter.CurrentHeading);
                teleporter.Position = positionUpdate;
            }
        }

        private void ModifyWorldBaseStateForNextTick()
        {
            state.World.CurrentTick++;
            state.World.Radius--;
        }

        private void ManageSupernova()
        {
            var firstHalfBoundary = 11; //(int)(engineConfig.MaxRounds * 0.25);
            var tickIsAfterFirstQuarter = state.World.CurrentTick > firstHalfBoundary;
            var tickIsBeforeLastQuarter = state.World.CurrentTick < engineConfig.MaxRounds * 0.75;
            if (!tickIsAfterFirstQuarter || !tickIsBeforeLastQuarter)
            {
                return;
            }

            if (supernovaSpawnTick == 0)
            {
                var gameMidpoint = engineConfig.MaxRounds / 2;
                var tickAddition = (int) (engineConfig.Supernova.Seed % firstHalfBoundary);
                supernovaSpawnTick = firstHalfBoundary + tickAddition + 5;
                Logger.LogDebug("SUPERNOVA", $"Supernova will spawn on tick {supernovaSpawnTick}");
            }

            if (state.World.CurrentTick == supernovaSpawnTick)
            {
                SpawnSupernova();
            }

            if (supernovaShouldDetonate)
            {
                DetonateSupernova();
            }

            if (supernovaLaunched)
            {
                Logger.LogDebug("SUPERNOVA", "Supernova launched, moving supernova");
                var supernova =
                    state.GameObjects.FirstOrDefault(go => go.GameObjectType == GameObjectType.SupernovaBomb);
                if (supernova == default)
                {
                    Logger.LogWarning("Supernova", "Supernova is marked as launched but no game object correlates");
                }

                var positionUpdate =
                    vectorCalculatorService.GetPositionFrom(supernova.Position, supernova.Speed,
                        supernova.CurrentHeading);
                supernova.Position = positionUpdate;
            }
        }

        private void SpawnSupernova()
        {
            var supernovaPickup = new GameObject
            {
                Id = Guid.NewGuid(),
                GameObjectType = GameObjectType.SupernovaPickup,
                Position = new Position(0, 0)
            };
            state.GameObjects.Add(supernovaPickup);
            Logger.LogDebug("SUPERNOVA", "Supernova Pickup spawned!");
            Logger.LogDebug("SUPERNOVA", $"Pickup Id: {supernovaPickup.Id.ToString()}");
            Logger.LogDebug("SUPERNOVA", $"Position: {supernovaPickup.Position.ToString()}");
        }

        private void UpdatePlayersExceedingWorldBounds()
        {
            foreach (var statePlayerObject in state.PlayerGameObjects)
            {
                if (vectorCalculatorService.IsInWorldBoundsWithOffset(statePlayerObject.Position,
                        statePlayerObject.Size, state.World.Radius) !=
                    true)
                {
                    statePlayerObject.Size -= 1;
                }

                if (statePlayerObject.Size < 5)
                {
                    Logger.LogInfo("BotDeath", "Bot shrunk too small from world bounds");
                    markedForRemoval.Add(statePlayerObject.Id);
                }

                UpdateBotSpeed(statePlayerObject);
            }
        }

        private void MarkExceedingWorldBoundsGameObjectsForRemoval()
        {
            foreach (var stateGameObject in state.GameObjects.Where(
                stateGameObject =>
                    !vectorCalculatorService.IsInWorldBounds(stateGameObject.Position, state.World.Radius)))
            {
                markedForRemoval.Add(stateGameObject.Id);
            }
        }

        private void UpdateWormholeSizeAndMarkExceedingWorldBounds()
        {
            foreach (var (item1, item2) in state.WormholePairs)
            {
                if (!vectorCalculatorService.IsInWorldBoundsWithOffset(item1.Position, item1.Size,
                        state.World.Radius) ||
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
        }

        private void HandleEffects()
        {
            var effectsToRemove = new List<ActiveEffect>();

            //REFACTOR INTO A SWITCH:
            foreach (var activeEffect in activeEffects)
            {
                var bot = state.PlayerGameObjects.Find(b => b.Id == activeEffect.Bot.Id);

                switch (activeEffect.Effect)
                {
                    /* Handle Afterburner costs. */
                    case Effects.Afterburner:
                        /* Only reduce bot size if it has an active Afterburner. */
                        if (bot != default)
                        {
                            bot.Size -= engineConfig.Afterburners.SizeConsumptionPerTick;
                        }

                        break;

                    /* Handle Gas Cloud effects. */
                    case Effects.GasCloud:
                        /* Only reduce bot size if it is in a Gas Cloud. */
                        if (GetCollisions(bot).All(c => c.GameObjectType != GameObjectType.GasCloud))
                        {
                            // This is state in place modification
                            effectsToRemove.Add(activeEffect);
                        }
                        else
                        {
                            bot.Size -= engineConfig.GasClouds.AffectPerTick;
                        }

                        break;

                    /* Handle Asteroid Field effects. */
                    case Effects.AsteroidField:
                        /* Only reduce bot speed if it is in an Asteroid Field. */
                        if (GetCollisions(bot).All(c => c.GameObjectType != GameObjectType.AsteroidField))
                        {
                            effectsToRemove.Add(activeEffect);
                        }
                        else
                        {
                            UpdateBotSpeed(bot);
                        }

                        break;

                    /* Handle Superfood duration reduction and removal if needed */
                    case Effects.Superfood:
                        /* Only reduce superfood duration. */
                        activeEffect.EffectDuration -= 1;

                        if (activeEffect.EffectDuration <= 0)
                        {
                            effectsToRemove.Add(activeEffect);
                        }

                        break;

                    case Effects.Shield:
                        activeEffect.EffectDuration -= 1;

                        if (activeEffect.EffectDuration <= 0)
                        {
                            effectsToRemove.Add(activeEffect);
                        }

                        break;
                }
            }

            effectsToRemove.ForEach(RemoveActiveEffect);
        }

        private void RemoveGameObjectsMarkedForRemoval()
        {
            foreach (var gameObject in markedForRemoval.Select(guid => state.GameObjects.Find(go => go.Id == guid))
                .Where(gameObject => gameObject != default))
            {
                state.GameObjects.Remove(gameObject);

                if (gameObject.GameObjectType == GameObjectType.Wormhole)
                {
                    state.WormholePairs.Remove(GetWormholePair(gameObject.Id));
                }
            }
        }

        private void RemoveAndPlaceBotsMarkedForRemoval()
        {
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
        }

        private void AddTorpedoSalvos()
        {
            if (state.World.CurrentTick % engineConfig.Torpedo.ChargeRate == 0)
            {
                foreach (var bot in state.PlayerGameObjects.Where(bot =>
                    bot.TorpedoSalvoCount < engineConfig.Torpedo.MaxChargeCount))
                {
                    bot.TorpedoSalvoCount++;
                }
            }
        }

        private void AddTeleporter()
        {
            if (state.World.CurrentTick % engineConfig.Teleport.ChargeRate == 0)
            {
                foreach (var bot in state.PlayerGameObjects.Where(bot =>
                    bot.TeleporterCount < engineConfig.Teleport.MaxChargeCount))
                {
                    bot.TeleporterCount++;
                }
            }
        }

        private void AddShieldCount()
        {
            if (state.World.CurrentTick % engineConfig.Shield.ChargeRate == 0)
            {
                foreach (var bot in state.PlayerGameObjects.Where(bot =>
                    bot.ShieldCount < engineConfig.Shield.MaxChargeCount))
                {
                    bot.ShieldCount++;
                }
            }
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
                bot.Effects &= ~activeEffect.Effect;
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

        /* I need to have this GetCollisions in the WorldStateService in order to check for collisions with Gas Clouds and Asteroid Fields every tick. */
        private List<GameObject> GetCollisions(BotObject bot)
        {
            if (bot == null)
            {
                return new List<GameObject>();
            }

            IList<GameObject> gameObjects = GetCurrentGameObjects();
            return gameObjects.Where(go => go.Id != bot.Id && vectorCalculatorService.HasOverlap(go, bot)).ToList();
        }
    }
}