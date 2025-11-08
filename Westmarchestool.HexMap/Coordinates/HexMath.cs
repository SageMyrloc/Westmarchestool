namespace Westmarchestool.HexMap.Coordinates
{
    /// <summary>
    /// Provides mathematical operations for hex coordinates
    /// </summary>
    public static class HexMath
    {
        /// <summary>
        /// Calculate the distance (in hexes) between two hexes
        /// </summary>
        public static int Distance(CubeHex a, CubeHex b)
        {
            return (Math.Abs(a.X - b.X)
                  + Math.Abs(a.Y - b.Y)
                  + Math.Abs(a.Z - b.Z)) / 2;
        }

        /// <summary>
        /// Calculate distance between two Axial hexes
        /// </summary>
        public static int Distance(AxialHex a, AxialHex b)
        {
            var cubeA = HexConverter.AxialToCube(a);
            var cubeB = HexConverter.AxialToCube(b);
            return Distance(cubeA, cubeB);
        }

        /// <summary>
        /// Get all hexes within a given range from center
        /// </summary>
        public static List<CubeHex> GetHexesInRange(CubeHex center, int range)
        {
            var results = new List<CubeHex>();

            for (int x = -range; x <= range; x++)
            {
                int yMin = Math.Max(-range, -x - range);
                int yMax = Math.Min(range, -x + range);

                for (int y = yMin; y <= yMax; y++)
                {
                    int z = -x - y;

                    results.Add(new CubeHex(
                        center.X + x,
                        center.Y + y,
                        center.Z + z
                    ));
                }
            }

            return results;
        }

        /// <summary>
        /// Get all hexes within range, accepting Axial center
        /// </summary>
        public static List<CubeHex> GetHexesInRange(AxialHex center, int range)
        {
            var cubeCenter = HexConverter.AxialToCube(center);
            return GetHexesInRange(cubeCenter, range);
        }

        /// <summary>
        /// Get only the hexes at exactly a given distance (the ring)
        /// </summary>
        public static List<CubeHex> GetHexRing(CubeHex center, int radius)
        {
            var results = new List<CubeHex>();

            if (radius == 0)
            {
                results.Add(center);
                return results;
            }

            var hex = new CubeHex(
                center.X + CubeHex.Directions[4].X * radius,
                center.Y + CubeHex.Directions[4].Y * radius,
                center.Z + CubeHex.Directions[4].Z * radius
            );

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

        /// <summary>
        /// Get hex ring, accepting Axial center
        /// </summary>
        public static List<CubeHex> GetHexRing(AxialHex center, int radius)
        {
            var cubeCenter = HexConverter.AxialToCube(center);
            return GetHexRing(cubeCenter, radius);
        }
    }
}