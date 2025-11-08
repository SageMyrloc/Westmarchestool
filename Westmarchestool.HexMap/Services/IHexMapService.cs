using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.HexMap.Entities;

namespace Westmarchestool.HexMap.Services
{
    /// <summary>
    /// Service interface for hex map operations
    /// </summary>
    public interface IHexMapService
    {
        // Basic hex operations
        Task<HexTile?> GetHexAsync(int q, int r);
        Task<HexTile?> GetHexAsync(AxialHex axial);
        Task<List<HexTile>> GetHexesInRangeAsync(AxialHex center, int range);
        Task<HexTile> CreateHexAsync(AxialHex position, string terrainType);

        // GM operations
        Task<List<HexTile>> GetGMMapAsync();
        Task<HexTile> UpdateHexAsync(int q, int r, string terrainType);

        // Town map operations
        Task<List<HexTile>> GetTownMapAsync();
        Task MarkHexAsPublicAsync(int q, int r);

        // Neighbor operations
        Task<List<HexTile>> GetNeighborsAsync(AxialHex hex);
        Task GenerateBorderHexesAsync(AxialHex center, int borderDistance);
    }
}