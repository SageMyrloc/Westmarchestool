namespace Westmarchestool.API.Hexmap.Coordinates
{
    /// <summary>
    /// Provides conversion functions between Cube and Axial hex coordinate systems.
    /// Cube: (X, Y, Z) - Best for calculations, requires X+Y+Z=0
    /// Axial: (Q, R) - Best for storage, only 2 integers needed
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
            int y = -x - z;  // Calculate the missing coordinate

            return new CubeHex(x, y, z);
        }

        /// <summary>
        /// Convert Axial coordinates (Q, R) to Cube, accepting raw integers
        /// </summary>
        public static CubeHex AxialToCube(int q, int r)
        {
            return AxialToCube(new AxialHex(q, r));
        }

        /// <summary>
        /// Convert Cube coordinates (X, Y, Z) to Axial coordinates (Q, R)
        /// Note: Y is discarded as it can be recalculated from Q and R
        /// </summary>
        public static AxialHex CubeToAxial(CubeHex cube)
        {
            int q = cube.X;
            int r = cube.Z;
            // Y is discarded (not needed for storage)

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