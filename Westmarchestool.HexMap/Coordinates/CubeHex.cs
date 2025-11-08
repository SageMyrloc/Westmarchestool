namespace Westmarchestool.HexMap.Coordinates
{
    public class CubeHex
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public static readonly CubeHex[] Directions = new CubeHex[]
        {
            new CubeHex(1, -1, 0),   // 0: East
            new CubeHex(1, 0, -1),   // 1: Northeast
            new CubeHex(0, 1, -1),   // 2: Northwest
            new CubeHex(-1, 1, 0),   // 3: West
            new CubeHex(-1, 0, 1),   // 4: Southwest
            new CubeHex(0, -1, 1)    // 5: Southeast
        };

        public CubeHex(int x, int y, int z)
        {
            if (x + y + z != 0)
            {
                throw new ArgumentException(
                    $"Invalid cube coordinates: {x} + {y} + {z} must equal 0"
                );
            }

            X = x;
            Y = y;
            Z = z;
        }

        public CubeHex GetNeighbor(int direction)
        {
            if (direction < 0 || direction > 5)
            {
                throw new ArgumentException("Direction must be between 0 and 5");
            }

            var dir = Directions[direction];
            return new CubeHex(X + dir.X, Y + dir.Y, Z + dir.Z);
        }

        public List<CubeHex> GetAllNeighbors()
        {
            var neighbors = new List<CubeHex>();
            for (int i = 0; i < 6; i++)
            {
                neighbors.Add(GetNeighbor(i));
            }
            return neighbors;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CubeHex other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override string ToString()
        {
            return $"CubeHex({X}, {Y}, {Z})";
        }

        public static bool operator ==(CubeHex? a, CubeHex? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(CubeHex? a, CubeHex? b)
        {
            return !(a == b);
        }
    }
}