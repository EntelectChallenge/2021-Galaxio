using System.Collections.Generic;

namespace NETCoreBot.Models
{
    public class GameState
    {
        public World World { get; set; }
        public List<GameObject> GameObjects { get; set; }
    }
}
