namespace Domain.Models
{
    public class World
    {
        public Position CenterPoint { get; set; }
        public int Radius { get; set; }
        public int CurrentTick { get; set; }
    }
}