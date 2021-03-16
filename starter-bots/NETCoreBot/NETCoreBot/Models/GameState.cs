using System.Collections.Generic;

namespace NETCoreBot.Models
{
    public class GameState
    {
        public World World { get; set; }
        public List<GameObject> GameObjects { get; set; } // [ size, x, y, type, id] DTO, non player objects
        public List<GameObject> PlayerGameObjects { get; set; } // [ size, x, y, speed, type, id] DTO
    }
}