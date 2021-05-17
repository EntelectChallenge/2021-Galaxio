using NETCoreBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using NETCoreBot.Enums;
using Newtonsoft.Json;

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
        private GameObject _target;
        private bool _targetIsPlayer = false;
        private GameObject _worldCenter;

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

        public bool ComputeNextPlayerAction(PlayerAction playerAction)
        {
            var actionId = 1;
            var heading = 90;

            if (!_gameState.PlayerGameObjects.Exists(b => b.Id == _bot.Id))
            {
                Console.WriteLine("I am no longer in the game state, and have been consumed");
                return false;
            }

            if (_target == null || _target == _worldCenter)
            {
                Console.WriteLine("No Current Target, resolving new target");
                heading = ResolveNewTarget();
            }
            else
            {
                var targetWithNewValues = _gameState.GameObjects.FirstOrDefault(go => go.Id == _target.Id) ??
                    _gameState.PlayerGameObjects.FirstOrDefault(go => go.Id == _target.Id);

                if (targetWithNewValues == default)
                {
                    Console.WriteLine("Old Target Invalid, resolving new target");
                    heading = ResolveNewTarget();
                }
                else
                {
                    Console.WriteLine("Previous Target exists, updating resolution");
                    _target = targetWithNewValues;

                    if (_target.Size < _bot.Size)
                    {
                        heading = GetDirection(_bot, _target);
                    }
                    else
                    {
                        Console.WriteLine("Previous Target larger than me, resolving new target");
                        heading = ResolveNewTarget();
                    }
                }
            }

            var distanceFromWorldCenter = GetDistanceBetween(
                new GameObject
                {
                    Position = _gameState.World.CenterPoint
                });

            if (distanceFromWorldCenter + (1.5 * _bot.Size) > _gameState.World.Radius)
            {
                _worldCenter = new GameObject
                {
                    Position = new Position
                    {
                        X = 0,
                        Y = 0
                    }
                };
                heading = GetDirection(
                    _bot,
                    _worldCenter);
                Console.WriteLine("Near the edge, going to the center");
                _target = _worldCenter;
            }

            if ((_targetIsPlayer || _target == _worldCenter) && _bot.Size > 20 && _bot.TorpedoSalvoCount > 0)
            {
                Console.WriteLine("Firing Torpedoes at target");
                actionId = 5;
            }

            playerAction.Action = actionId;
            playerAction.Heading = heading;

            lastAction = playerAction;
            timeSinceLastAction = 0;

            _playerAction = playerAction;

            Console.WriteLine("Player action:" + playerAction.Action + ":" + playerAction.Heading);
            return true;
        }

        private int ResolveNewTarget()
        {
            int heading;
            var nearestFood = _gameState.GameObjects.Where(go => go.GameObjectType == ObjectTypes.Food)
                .OrderBy(GetDistanceBetween)
                .FirstOrDefault();
            var nearestPlayer = _gameState.PlayerGameObjects.Where(bot => bot.Id != _bot.Id).OrderBy(GetDistanceBetween).First();
            var nearestWormhole = _gameState.GameObjects.Where(go => go.GameObjectType == ObjectTypes.Wormhole)
                .OrderBy(GetDistanceBetween)
                .FirstOrDefault();

            var directionToNearestPlayer = GetDirection(_bot, nearestPlayer);
            var directionToNearestFood = GetDirection(_bot, nearestFood);

            if (nearestPlayer.Size > _bot.Size)
            {
                heading = GetAttackerResolution(_bot, nearestPlayer, nearestFood);
                _targetIsPlayer = false;
            }
            else if (nearestPlayer.Size < _bot.Size)
            {
                heading = GetDirection(_bot, nearestPlayer);
                _target = nearestPlayer;
                _targetIsPlayer = true;
                Console.WriteLine("Chasing Smaller Player");
            }
            else if (nearestFood != null)
            {
                heading = GetDirection(_bot, nearestFood);
                _target = nearestFood;
                _targetIsPlayer = false;
                Console.WriteLine("Going for a Feeding");
            }
            else
            {
                _target = _worldCenter;
                heading = GetDirection(
                    _bot,
                    _worldCenter);
                _targetIsPlayer = false;
                Console.WriteLine("Couldn't find anything, going to the center");
            }

            if (_target == _worldCenter)
            {
                heading = GetDirection(_bot, nearestPlayer);
            }

            return heading;
        }

        private int GetAttackerResolution(GameObject bot, GameObject attacker, GameObject closestFood)
        {
            if (closestFood == null)
            {
                return GetOppositeDirection(bot, attacker);
            }

            var distanceToAttacker = GetDistanceBetween(attacker);
            var distanceBetweenAttackerAndFood = GetDistanceBetween(attacker, closestFood);

            if (distanceToAttacker > attacker.Speed &&
                distanceBetweenAttackerAndFood > distanceToAttacker)
            {
                Console.WriteLine("Atk is far, going for food");
                return GetDirection(_bot, closestFood);
            }
            else
            {
                Console.WriteLine("Running");
                return GetOppositeDirection(bot, attacker);
            }
        }

        private int GetOppositeDirection(GameObject gameObject1, GameObject gameObject2)
        {
            return ToDegrees(Math.Atan2(gameObject2.Position.Y - gameObject1.Position.Y, gameObject2.Position.X - gameObject1.Position.X));
        }

        private int GetDirection(GameObject bot, GameObject gameObject)
        {
            var cartesianDegrees = ToDegrees(Math.Atan2(gameObject.Position.Y - bot.Position.Y, gameObject.Position.X - bot.Position.X));
            return cartesianDegrees = (cartesianDegrees + 360) % 360;
        }

        private int ToDegrees(double v)
        {
            return (int) (v * (180 / Math.PI));
        }

        private double GetDistanceBetween(GameObject bot, GameObject go)
        {
            var triangleX = Math.Abs(bot.Position.X - go.Position.X);
            var triangleY = Math.Abs(bot.Position.Y - go.Position.Y);
            return (int) Math.Ceiling(Math.Sqrt(triangleX * triangleX + triangleY * triangleY));
        }

        private double GetDistanceBetween(GameObject go)
        {
            return GetDistanceBetween(_bot, go);
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