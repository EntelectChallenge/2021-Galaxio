using System;

namespace Domain.Models
{
    public class Position: IEquatable<Position>
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

        public override int GetHashCode()
        {
            return X * 0x00010000 + Y;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Position);
        }

        public bool Equals(Position p)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (X == p.X) && (Y == p.Y);
        }

        public static bool operator ==(Position lhs, Position rhs)
        {
            return !Object.ReferenceEquals(lhs, null) ? lhs.Equals(rhs) : Object.ReferenceEquals(rhs, null);
        }

        public static bool operator !=(Position lhs, Position rhs)
        {
            return !(lhs == rhs);
        }
    }
}