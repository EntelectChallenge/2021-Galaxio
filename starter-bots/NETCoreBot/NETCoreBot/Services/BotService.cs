using NETCoreBot.Enums;
using NETCoreBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETCoreBot.Services
{
    public class BotService
    {
        private GameObject _bot;
        private PlayerAction _playerAction;
        private GameState _gameState;

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
            playerAction.Action = PlayerActions.Forward;
            playerAction.Heading = new Random().Next(0, 359);

            if (_gameState.GameObjects != null)
            {
                var foodList = _gameState.GameObjects
                    .Where(item => item.GameObjectType == ObjectTypes.Food)
                    .OrderBy(item => GetDistanceBetween(_bot, item))
                    .ToList();
            
                playerAction.Heading = GetHeadingBetween(foodList.First());
            }

            _playerAction = playerAction;
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
        
        private double GetDistanceBetween(GameObject object1, GameObject object2)
        {
            var triangleX = Math.Abs(object1.Position.X - object2.Position.X);
            var triangleY = Math.Abs(object1.Position.Y - object2.Position.Y);
            return Math.Sqrt(triangleX * triangleX + triangleY * triangleY);
        }
        
        private int GetHeadingBetween(GameObject otherObject)
        {
            var direction = ToDegrees(Math.Atan2(otherObject.Position.Y - _bot.Position.Y,
                                                    otherObject.Position.X - _bot.Position.X));
            return (direction + 360) % 360;
        }
        
        private int ToDegrees(double v)
        {
            return (int) (v * (180 / Math.PI));
        }
    }
}