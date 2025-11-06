namespace Westmarchestool.API.Hexmap.Coordinates
{
    public class AxialHex
    {
        public int Q { get; }
        public int R { get; }

        // The 6 direction vectors (only 2 coordinates each)
        public static readonly AxialHex[] Directions = new AxialHex[]
        {
            new AxialHex(1, 0),    // 0: East
            new AxialHex(1, -1),   // 1: Northeast
            new AxialHex(0, -1),   // 2: Northwest
            new AxialHex(-1, 0),   // 3: West
            new AxialHex(-1, 1),   // 4: Southwest
            new AxialHex(0, 1)     // 5: Southeast
        };

        public AxialHex(int q, int r)
        {
            Q = q;
            R = r;
        }

        // Get one neighbor by direction (0-5)
        public AxialHex GetNeighbor(int direction)
        {
            if (direction < 0 || direction > 5)
            {
                throw new ArgumentException("Direction must be between 0 and 5");
            }

            var dir = Directions[direction];
            return new AxialHex(Q + dir.Q, R + dir.R);
        }

        // Get all 6 neighbors at once
        public List<AxialHex> GetAllNeighbors()
        {
            var neighbors = new List<AxialHex>();
            for (int i = 0; i < 6; i++)
            {
                neighbors.Add(GetNeighbor(i));
            }
            return neighbors;
        }

        // Compare two hexes
        public override bool Equals(object? obj)
        {
            if (obj is AxialHex other)
            {
                return Q == other.Q && R == other.R;
            }
            return false;
        }

        // Required for dictionary keys
        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R);
        }

        // Print coordinates for debugging
        public override string ToString()
        {
            return $"AxialHex({Q}, {R})";
        }

        // Allow using == operator
        public static bool operator ==(AxialHex? a, AxialHex? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(AxialHex? a, AxialHex? b)
        {
            return !(a == b);
        }
    }
}