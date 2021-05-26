namespace Domain.Models
{
    public class MovableGameObject : GameObject, IMovable
    {
        public bool IsMoving { get; set; }
    }
}