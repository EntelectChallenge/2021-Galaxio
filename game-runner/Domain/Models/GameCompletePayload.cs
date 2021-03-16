using System.Collections.Generic;

namespace Domain.Models
{
    public class GameCompletePayload
    {
        public int TotalTicks { get; set; }
        public List<PlayerResult> Players { get; set; }
        public List<int> WorldSeeds { get; set; }
        public GameObject WinningBot { get; set; }
    }
}