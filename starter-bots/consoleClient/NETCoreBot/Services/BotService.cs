using NETCoreBot.Models;
using System;

namespace NETCoreBot.Services
{
    public class BotService
    {
        private GameObject _bot;
        private PlayerAction _playerAction;
        private GameState _gameState;

        public BotService()
        {
            _bot = new GameObject();
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

        public void SetPlayerAction(PlayerAction playerAction)
        {
            var random = new Random();
            var actionId = random.Next(1, 10);
            var heading = random.Next(0, 360);

            playerAction.ActionId = actionId;
            playerAction.Heading = heading;

            _playerAction = playerAction;
        }

        public GameState GetGameState()
        {
            return _gameState;
        }

        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }
    }
}
