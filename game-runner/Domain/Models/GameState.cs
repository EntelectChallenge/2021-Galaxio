using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class GameState
    {
        public World World { get; set; }
        public List<GameObject> GameObjects { get; set; } // [ size, x, y, type, id] DTO, non player objects
        public List<BotObject> PlayerGameObjects { get; set; } // [ size, x, y, speed, type, id] DTO
        public List<Tuple<GameObject, GameObject>> WormholePairs { get; set; }
    }
}