namespace Westmarchestool.HexMap.Coordinates
{
    /// <summary>
    /// Utility class for hexagonal grid mathematics.
    /// </summary>
    public static class HexMath
    {
        // Six directions in cube coordinates (flat-top hexes)
        private static readonly HexCoordinate[] Directions = new[]
        {
            new HexCoordinate(1, 0),   // East
            new HexCoordinate(1, -1),  // Northeast
            new HexCoordinate(0, -1),  // Northwest
            new HexCoordinate(-1, 0),  // West
            new HexCoordinate(-1, 1),  // Southwest
            new HexCoordinate(0, 1)    // Southeast
        };

        /// <summary>
        /// Calculate Manhattan distance between two hexes.
        /// </summary>
        public static int Distance(HexCoordinate a, HexCoordinate b)
        {
            return (Math.Abs(a.Q - b.Q)
                  + Math.Abs(a.Q + a.R - b.Q - b.R)
                  + Math.Abs(a.R - b.R)) / 2;
        }

        /// <summary>
        /// Get the neighbor hex in a specific direction (0-5).
        /// </summary>
        public static HexCoordinate GetNeighbor(HexCoordinate hex, int direction)
        {
            if (direction < 0 || direction > 5)
                throw new ArgumentOutOfRangeException(nameof(direction), "Direction must be 0-5");

            return hex + Directions[direction];
        }

        /// <summary>
        /// Get all six neighbors of a hex.
        /// </summary>
        public static IEnumerable<HexCoordinate> GetNeighbors(HexCoordinate hex)
        {
            for (int i = 0; i < 6; i++)
            {
                yield return GetNeighbor(hex, i);
            }
        }

        /// <summary>
        /// Get all hexes within a certain range (ring).
        /// </summary>
        public static IEnumerable<HexCoordinate> GetHexesInRange(HexCoordinate center, int range)
        {
            var results = new HashSet<HexCoordinate>();

            for (int q = -range; q <= range; q++)
            {
                for (int r = Math.Max(-range, -q - range); r <= Math.Min(range, -q + range); r++)
                {
                    results.Add(new HexCoordinate(center.Q + q, center.R + r));
                }
            }

            return results;
        }

        /// <summary>
        /// Get hexes in a specific ring around center (exact distance).
        /// </summary>
        public static IEnumerable<HexCoordinate> GetRing(HexCoordinate center, int radius)
        {
            if (radius == 0)
            {
                yield return center;
                yield break;
            }

            var hex = center + new HexCoordinate(-radius, radius);

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    yield return hex;
                    hex = GetNeighbor(hex, i);
                }
            }
        }
    }
}