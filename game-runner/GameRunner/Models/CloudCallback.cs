using System.Collections.Generic;

namespace GameRunner.Models
{
    public class CloudCallback
    {
        public string MatchId { get; set; }
        public string MatchStatus { get; set; }
        public string MatchStatusReason { get; set; }
        public string Seed { get; set; }
        public string Ticks { get; set; }
        public List<CloudPlayer> Players { get; set; }
    }
}