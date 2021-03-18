using System;
using Domain.Models;
using Domain.Services;
using Engine.Interfaces;

namespace Engine.Services
{
    public class VectorCalculatorService : IVectorCalculatorService
    {
        public Position MovePlayerObject(Position startPosition, int distance, int heading)
        {
            var resultingHeading = ConstrainHeading(heading);
            return QuadrantMath(startPosition, distance, resultingHeading);
        }

        public Position GetStartPosition(int startRadius, int heading) => QuadrantMath(new Position(0, 0), startRadius, heading);

        public Position GetPositionFrom(Position position, int distance, int heading) => QuadrantMath(position, distance, heading);

        public bool HasOverlap(GameObject go, GameObject bot)
        {
            var distanceBetween = GetDistanceBetween(bot.Position, go.Position);
            var isOverlapping = distanceBetween - (bot.Size + go.Size) < 0;
            return isOverlapping;
        }

        public bool IsInWorldBounds(Position position, int worldRadius)
        {
            var distanceFromBotToWorldCenter = GetDistanceBetween(position, new Position());
            return distanceFromBotToWorldCenter <= worldRadius;
        }

        public bool IsInWorldBoundsWithOffset(Position position, int offset, int worldRadius)
        {
            var distanceFromBotToWorldCenter = GetDistanceBetween(position, new Position());
            return distanceFromBotToWorldCenter + offset <= worldRadius;
        }

        public int GetDistanceBetween(Position botPosition, Position goPosition)
        {
            var triangleX = Math.Abs(botPosition.X - goPosition.X);
            var triangleY = Math.Abs(botPosition.Y - goPosition.Y);
            return (int) Math.Round(Math.Sqrt(triangleX * triangleX + triangleY * triangleY), 0);
        }

        public Position GetNewPlayerStartingPosition(int playerCount, int botCount, int startRadius)
        {
            if (playerCount >= botCount)
            {
                Logger.LogError("VectorCalculation", $"Current PlayerCount was equal to or Higher than BotCount. PlayerCount: {playerCount}, BotCount: {botCount}");
            }
            var degreeSeparation = 360 / botCount;
            var currentDegree = (playerCount + 1) * degreeSeparation;
            return GetStartPosition(startRadius, currentDegree);
        }

        public int ReverseHeading(int heading)
        {
            heading += 180;
            return ConstrainHeading(heading);
        }

        private Position QuadrantMath(Position startPosition, int speed, int headingDegree)
        {
            headingDegree = ConstrainHeading(headingDegree);
            var headingRadians = ConvertToRadians(headingDegree);
            var endPosition = new Position();

            if (headingDegree <= 90)
            {
                endPosition.X = startPosition.X + (int) Math.Round(speed * Math.Cos(headingRadians), 0);
                endPosition.Y = startPosition.Y + (int) Math.Round(speed * Math.Sin(headingRadians), 0);

                return endPosition;
            }

            if (headingDegree <= 180)
            {
                endPosition.X = startPosition.X - (int) Math.Round(speed * Math.Cos(Math.PI - headingRadians), 0);
                endPosition.Y = startPosition.Y + (int) Math.Round(speed * Math.Sin(Math.PI - headingRadians), 0);

                return endPosition;
            }

            if (headingDegree <= 270)
            {
                endPosition.X = startPosition.X - (int) Math.Round(speed * Math.Cos(headingRadians - Math.PI), 0);
                endPosition.Y = startPosition.Y - (int) Math.Round(speed * Math.Sin(headingRadians - Math.PI), 0);

                return endPosition;
            }

            if (headingDegree <= 360)
            {
                endPosition.X = startPosition.X + (int) Math.Round(speed * Math.Cos(2 * Math.PI - headingRadians), 0);
                endPosition.Y = startPosition.Y - (int) Math.Round(speed * Math.Sin(2 * Math.PI - headingRadians), 0);

                return endPosition;
            }

            throw new InvalidOperationException("Unsupported Heading Supplied");
        }

        private int ConstrainHeading(int heading)
        {
            while (heading < 0)
            {
                heading += 360;
            }

            while (heading > 360)
            {
                heading -= 360;
            }

            return heading;
        }

        private double ConvertToRadians(int heading) => heading * Math.PI / 180;
    }
}