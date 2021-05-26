namespace Domain.Models
{
    public interface IMovable
    {
        public int Speed { get; set; }
        public int CurrentHeading { get; set; }
    }
}