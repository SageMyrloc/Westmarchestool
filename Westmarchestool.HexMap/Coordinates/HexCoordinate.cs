namespace Westmarchestool.HexMap.Coordinates
{
    /// <summary>
    /// Represents a hexagonal coordinate using both cube and axial coordinate systems.
    /// Cube coordinates (q, r, s) are used for operations.
    /// Axial coordinates (q, r) are used for storage.
    /// Reference: https://www.redblobgames.com/grids/hexagons/
    /// </summary>
    public struct HexCoordinate : IEquatable<HexCoordinate>
    {
        // Axial coordinates (used for storage)
        public int Q { get; }
        public int R { get; }

        // Cube coordinate S (derived, not stored)
        public int S => -Q - R;

        public HexCoordinate(int q, int r)
        {
            Q = q;
            R = r;
        }

        // Create from cube coordinates
        public static HexCoordinate FromCube(int q, int r, int s)
        {
            // Validate cube constraint
            if (q + r + s != 0)
                throw new ArgumentException("Cube coordinates must satisfy q + r + s = 0");

            return new HexCoordinate(q, r);
        }

        // Equality
        public bool Equals(HexCoordinate other)
        {
            return Q == other.Q && R == other.R;
        }

        public override bool Equals(object? obj)
        {
            return obj is HexCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R);
        }

        public static bool operator ==(HexCoordinate left, HexCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HexCoordinate left, HexCoordinate right)
        {
            return !left.Equals(right);
        }

        // String representation
        public override string ToString()
        {
            return $"({Q}, {R})";
        }

        // Addition (for movement)
        public static HexCoordinate operator +(HexCoordinate a, HexCoordinate b)
        {
            return new HexCoordinate(a.Q + b.Q, a.R + b.R);
        }

        // Subtraction (for direction)
        public static HexCoordinate operator -(HexCoordinate a, HexCoordinate b)
        {
            return new HexCoordinate(a.Q - b.Q, a.R - b.R);
        }
    }
}