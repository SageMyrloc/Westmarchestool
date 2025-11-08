namespace Westmarchestool.HexMap.Coordinates
{
    /// <summary>
    /// Provides conversion functions between Cube and Axial hex coordinate systems
    /// </summary>
    public static class HexConverter
    {
        /// <summary>
        /// Convert Axial coordinates (Q, R) to Cube coordinates (X, Y, Z)
        /// </summary>
        public static CubeHex AxialToCube(AxialHex axial)
        {
            int x = axial.Q;
            int z = axial.R;
            int y = -x - z;

            return new CubeHex(x, y, z);
        }

        /// <summary>
        /// Convert Axial coordinates to Cube, accepting raw integers
        /// </summary>
        public static CubeHex AxialToCube(int q, int r)
        {
            return AxialToCube(new AxialHex(q, r));
        }

        /// <summary>
        /// Convert Cube coordinates (X, Y, Z) to Axial coordinates (Q, R)
        /// </summary>
        public static AxialHex CubeToAxial(CubeHex cube)
        {
            int q = cube.X;
            int r = cube.Z;

            return new AxialHex(q, r);
        }

        /// <summary>
        /// Convert Cube coordinates to Axial, accepting raw integers
        /// </summary>
        public static AxialHex CubeToAxial(int x, int y, int z)
        {
            return CubeToAxial(new CubeHex(x, y, z));
        }
    }
}