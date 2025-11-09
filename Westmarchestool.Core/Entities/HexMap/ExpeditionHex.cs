using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.Core.Entities.HexMap
{
    /// <summary>
    /// Represents a hex discovered/marked by an expedition.
    /// This is the expedition's temporary map data.
    /// </summary>
    public class ExpeditionHex
    {
        public int Id { get; set; }

        // Foreign key to expedition
        [Required]
        public int ExpeditionId { get; set; }
        public Expedition Expedition { get; set; } = null!;

        // Axial coordinates
        [Required]
        public int Q { get; set; }

        [Required]
        public int R { get; set; }

        // Terrain marked by expedition leader
        [Required]
        public TerrainType Terrain { get; set; }

        // When was this hex marked/discovered
        public DateTime DiscoveredDate { get; set; } = DateTime.UtcNow;

        // Notes (visible to all expedition members)
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}