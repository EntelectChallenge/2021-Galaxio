namespace Domain.Models
{
    public class Position
    {
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position()
        {
            X = 0;
            Y = 0;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}