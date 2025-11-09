using Westmarchestool.Core.Entities.HexMap;

namespace Westmarchestool.HexMap.Services
{
    public interface IMapMergeService
    {
        /// <summary>
        /// Merge an expedition's map to the Town Map.
        /// Creates discovery history entries and flags disputes.
        /// </summary>
        Task<MapMergeResult> MergeExpeditionToTownMapAsync(int expeditionId);

        /// <summary>
        /// Merge a single player's map when they leave an expedition early.
        /// </summary>
        Task<MapMergeResult> MergePlayerMapToTownMapAsync(int expeditionId, int userId);

        /// <summary>
        /// Get all disputed hexes on the Town Map.
        /// </summary>
        Task<List<TownMapHex>> GetDisputedHexesAsync();

        /// <summary>
        /// Manually resolve a disputed hex (GM only).
        /// Sets the terrain and marks as verified.
        /// </summary>
        Task<bool> ResolveDisputeAsync(int townMapHexId, TerrainType correctTerrain, int resolvedByUserId);
    }

    /// <summary>
    /// Result of a map merge operation.
    /// </summary>
    public class MapMergeResult
    {
        public int HexesAdded { get; set; }
        public int HexesConfirmed { get; set; }
        public int DisputesCreated { get; set; }
        public List<TownMapHex> NewDisputes { get; set; } = new();
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}