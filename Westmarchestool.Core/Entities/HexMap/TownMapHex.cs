using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.Core.Entities.HexMap
{
    /// <summary>
    /// Represents a hex on the Town Map (community knowledge, can be wrong).
    /// </summary>
    public class TownMapHex
    {
        public int Id { get; set; }

        // Axial coordinates
        [Required]
        public int Q { get; set; }

        [Required]
        public int R { get; set; }

        // Current believed terrain
        [Required]
        public TerrainType Terrain { get; set; }

        // Hex status
        [Required]
        public HexStatus Status { get; set; } = HexStatus.Unexplored;

        // When was this hex first discovered
        public DateTime? FirstDiscoveredDate { get; set; }

        // Last time this hex was updated/verified
        public DateTime? LastVerifiedDate { get; set; }

        // Navigation: Discovery history for this hex
        public ICollection<HexDiscoveryHistory> DiscoveryHistory { get; set; } = new List<HexDiscoveryHistory>();

        // Navigation: POIs players think are on this hex
        public ICollection<PointOfInterest> PointsOfInterest { get; set; } = new List<PointOfInterest>();
    }
}