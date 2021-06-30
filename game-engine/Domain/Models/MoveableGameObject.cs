namespace Domain.Models
{
    public class MovableGameObject : GameObject, IMovable
    {
        public bool ShouldCalculateCollisionPaths { get; set; }
    }
}