using NETCoreBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using NETCoreBot.Enums;

namespace NETCoreBot.Services
{
    public class BotService
    {
        private GameObject _bot;
        private PlayerAction _playerAction;
        private GameState _gameState;
        private int _searchRadiusModifier = 200;
        private bool _afterburnerOn = false;
        private PlayerAction lastAction;
        private int timeSinceLastAction;

        public BotService()
        {
            _playerAction = new PlayerAction();
            _gameState = new GameState();
        }

        public GameObject GetBot()
        {
            return _bot;
        }

        public PlayerAction GetPlayerAction()
        {
            return _playerAction;
        }

        public void SetBot(GameObject bot)
        {
            _bot = bot;
        }

        public void ComputeNextPlayerAction(PlayerAction playerAction)
        {
            var searchRadius = _bot.Size + _searchRadiusModifier;
            var foodInRange = new List<GameObject>();
            var wormHoleInRange = new List<GameObject>();
            var largerPlayerInRange = new List<GameObject>();
            var smallerPlayerInRange = new List<GameObject>();
            var actionId = 1; // always move forward
            var heading = 90;


            if (searchRadius > _bot.Size + 200)
            {
                _searchRadiusModifier = 200;
                searchRadius = _bot.Size + 50;
            }

            if (_gameState.GameObjects == null)
            {
                Console.WriteLine("No game objects");
                return;
            }

            foreach (var go in _gameState.GameObjects.Where(go => distance(go) < searchRadius))
            {
                switch (go.GameObjectType)
                {
                    case Enums.ObjectTypes.Food:
                        foodInRange.Add(go);
                        break;
                    case ObjectTypes.Wormhole:
                        wormHoleInRange.Add(go);
                        break;
                }
            }

            foreach (var go in _gameState.PlayerGameObjects.Where(go => distance(go) < searchRadius))
            {
                if (go.Id == _bot.Id)
                {
                    break;
                }

                if (go.Size >= _bot.Size)
                {
                    largerPlayerInRange.Add(go);
                }
                else
                {
                    smallerPlayerInRange.Add(go);
                }
            }

            Console.WriteLine(
                $"[{_bot.Id.ToString().Substring(0, 4)}] OOI: {largerPlayerInRange.Count} Larger Players, {smallerPlayerInRange.Count} Smaller Players, {foodInRange.Count} Food");

            if (largerPlayerInRange.Count > 0)
            {
                heading = GetAttackerResolution(_bot, largerPlayerInRange.First(), foodInRange);

                if (_bot.Size > 10)
                {
                    actionId = 3;
                    _afterburnerOn = true;
                }
                Console.WriteLine("Running");
            }
            else if (smallerPlayerInRange.Count > 0)
            {
                heading = GetDirection(_bot, smallerPlayerInRange.First());
                Console.WriteLine("Chasing Smaller Player");
            }
            else if (foodInRange.Count > 0)
            {
                heading = GetDirection(_bot, foodInRange.First());
                Console.WriteLine("Going for a Feeding");
            }
            else
            {
                _searchRadiusModifier += 100;
                heading = GetDirection(
                    _bot,
                    new GameObject
                    {
                        Position = new Position
                        {
                            X = 0,
                            Y = 0
                        }
                    });
                Console.WriteLine(
                    $"Couldn't find anything, going to the center. Increased Search radius to: {_bot.Size + _searchRadiusModifier}");
            }

            if (_bot.Size < 10 && _afterburnerOn)
            {
                _afterburnerOn = false;
                actionId = 4;
            }

            playerAction.Action = actionId;
            playerAction.Heading = heading;

            lastAction = playerAction;
            timeSinceLastAction = 0;

            _playerAction = playerAction;

            Console.WriteLine("Player action:" + playerAction.Action + ":" + playerAction.Heading);
            Console.WriteLine("Position:" + _bot.Position.X + ":" + _bot.Position.Y);
        }

        private int GetAttackerResolution(GameObject bot, GameObject attacker, List<GameObject> foodInRange)
        {
            var closestFood = foodInRange.OrderBy(distance).FirstOrDefault();
            if (closestFood == null)
            {
                return GetOppositeDirection(bot, attacker);
            }

            var distanceToAttacker = distance(attacker);
            var distanceBetweenAttackerAndFood = distance(attacker, closestFood);

            if (distanceToAttacker > attacker.Speed &&
                distanceBetweenAttackerAndFood > distanceToAttacker)
            {
                return GetDirection(_bot, closestFood);
            }
            else
            {
                return GetOppositeDirection(bot, attacker);
            }
        }

        private int GetOppositeDirection(GameObject gameObject1, GameObject gameObject2)
        {
            return ToDegrees(Math.Atan2(gameObject2.Position.Y - gameObject1.Position.Y, gameObject2.Position.X - gameObject1.Position.X));
        }

        private int GetDirection(GameObject bot, GameObject gameObject)
        {
            Console.WriteLine($"Getting heading from me to {gameObject.Position.X}:{gameObject.Position.Y}");
            var cartesianDegrees = ToDegrees(
                Math.Atan2(gameObject.Position.Y - bot.Position.Y, gameObject.Position.X - bot.Position.X));
            return cartesianDegrees = (cartesianDegrees + 360) % 360;
        }

        private int ToDegrees(double v)
        {
            return (int) (v * (180 / Math.PI));
        }

        private double distance(GameObject bot, GameObject go)
        {
            var triangleX = Math.Abs(bot.Position.X - go.Position.X);
            var triangleY = Math.Abs(bot.Position.Y - go.Position.Y);
            return (int) Math.Ceiling(Math.Sqrt(triangleX * triangleX + triangleY * triangleY));
        }

        private double distance(GameObject go)
        {
            return distance(_bot, go);
        }

        public GameState GetGameState()
        {
            return _gameState;
        }

        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
            UpdateSelfState();
        }

        private void UpdateSelfState()
        {
            _bot = _gameState.PlayerGameObjects.FirstOrDefault(go => go.Id == _bot.Id);
        }
    }
}