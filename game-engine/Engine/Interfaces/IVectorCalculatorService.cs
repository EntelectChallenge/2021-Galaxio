using Domain.Models;

namespace Engine.Interfaces

{
    public interface IVectorCalculatorService
    {
        Position MovePlayerObject(Position startPosition, int distance, int heading);
        public Position GetStartPosition(int startRadius, int heading);
        bool HasOverlap(GameObject go, GameObject bot);
        bool IsInWorldBounds(Position position, int worldRadius);
        bool IsInWorldBoundsWithOffset(Position position, int offset, int worldRadius);
        Position GetPositionFrom(Position position, int distance, int heading);
        int GetDistanceBetween(Position position1, Position position2);
        Position GetNewPlayerStartingPosition(int playerCount, int botCount, int startRadius);
        int ReverseHeading(int heading);
    }
}