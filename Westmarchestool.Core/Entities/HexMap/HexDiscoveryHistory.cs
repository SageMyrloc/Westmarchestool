using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.Core.Entities.HexMap
{
    /// <summary>
    /// Tracks the discovery history of a Town Map hex.
    /// Records all expeditions that have explored this hex and what they found.
    /// </summary>
    public class HexDiscoveryHistory
    {
        public int Id { get; set; }

        // Foreign key to Town Map hex
        [Required]
        public int TownMapHexId { get; set; }
        public TownMapHex TownMapHex { get; set; } = null!;

        // Which expedition made this discovery
        [Required]
        public int ExpeditionId { get; set; }
        public Expedition Expedition { get; set; } = null!;

        // What terrain did they report
        [Required]
        public TerrainType ReportedTerrain { get; set; }

        // When was this discovery made
        public DateTime DiscoveryDate { get; set; } = DateTime.UtcNow;

        // Was this a verification expedition?
        public bool IsVerification { get; set; } = false;

        // Optional notes from the expedition
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}