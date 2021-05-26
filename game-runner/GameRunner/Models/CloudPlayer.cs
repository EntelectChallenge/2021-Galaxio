namespace GameRunner.Models
{
    public class CloudPlayer
    {
        public string PlayerParticipantId { get; set; }
        public string GamePlayerId { get; set; }
        public int FinalScore { get; set; }
        public int Placement { get; set; }
        public int Seed { get; set; }
        public int MatchPoints { get; set; }
    }
}