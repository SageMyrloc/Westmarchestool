namespace Westmarchestool.API.Hexmap.Coordinates
{
    /// Provides mathematical operations for hex coordinates

    public static class HexMath
    {
        /// Calculate the distance (in hexes) between two hexes
        /// Works with Cube coordinates for clean math
        public static int Distance(CubeHex a, CubeHex b)
        {
            // 3D Manhattan distance divided by 2
            // (Each hex step changes 2 coordinates by 1 each)
            return (Math.Abs(a.X - b.X)
                  + Math.Abs(a.Y - b.Y)
                  + Math.Abs(a.Z - b.Z)) / 2;
        }

        /// Calculate distance between two Axial hexes
        /// (Converts to Cube internally for easier math)
        public static int Distance(AxialHex a, AxialHex b)
        {
            var cubeA = HexConverter.AxialToCube(a);
            var cubeB = HexConverter.AxialToCube(b);
            return Distance(cubeA, cubeB);
        }

        /// Get all hexes within a given range (distance) from center
        /// This is crucial for:
        /// - Generating exploration borders
        /// - Fog-of-war visibility
        /// - Area-of-effect spells
        /// - Movement range display
        public static List<CubeHex> GetHexesInRange(CubeHex center, int range)
        {
            var results = new List<CubeHex>();

            // Loop through all possible X values
            for (int x = -range; x <= range; x++)
            {
                // For each X, calculate valid Y range
                // (must maintain X + Y + Z = 0 constraint)
                int yMin = Math.Max(-range, -x - range);
                int yMax = Math.Min(range, -x + range);

                for (int y = yMin; y <= yMax; y++)
                {
                    // Calculate Z from X and Y
                    int z = -x - y;

                    // Create hex relative to origin, then offset by center
                    results.Add(new CubeHex(
                        center.X + x,
                        center.Y + y,
                        center.Z + z
                    ));
                }
            }

            return results;
        }

        /// Get all hexes within range, accepting Axial center
        public static List<CubeHex> GetHexesInRange(AxialHex center, int range)
        {
            var cubeCenter = HexConverter.AxialToCube(center);
            return GetHexesInRange(cubeCenter, range);
        }

        /// Get only the hexes at exactly a given distance (the ring)
        /// Useful for generating just the border hexes
        public static List<CubeHex> GetHexRing(CubeHex center, int radius)
        {
            var results = new List<CubeHex>();

            if (radius == 0)
            {
                results.Add(center);
                return results;
            }

            // Start at one edge of the ring
            var hex = new CubeHex(
                center.X + CubeHex.Directions[4].X * radius,
                center.Y + CubeHex.Directions[4].Y * radius,
                center.Z + CubeHex.Directions[4].Z * radius
            );

            // Walk around the ring in 6 directions
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    results.Add(hex);
                    hex = hex.GetNeighbor(i);
                }
            }

            return results;
        }

        /// Get hex ring, accepting Axial center
        /// </>
        public static List<CubeHex> GetHexRing(AxialHex center, int radius)
        {
            var cubeCenter = HexConverter.AxialToCube(center);
            return GetHexRing(cubeCenter, radius);
        }
    }
}
