using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.Core.Entities.HexMap
{
    /// <summary>
    /// Represents a hex on the GM Master Map (source of truth).
    /// </summary>
    public class GMMapHex
    {
        public int Id { get; set; }

        // Axial coordinates
        [Required]
        public int Q { get; set; }

        [Required]
        public int R { get; set; }

        // Terrain (always accurate)
        [Required]
        public TerrainType Terrain { get; set; }

        // Is this hex manually set by GM (locks from regeneration)?
        public bool IsManuallySet { get; set; } = false;

        // GM notes (not visible to players)
        [MaxLength(1000)]
        public string? GmNotes { get; set; }

        // When was this hex generated/created
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // When was it last modified
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation: POIs on this hex
        public ICollection<PointOfInterest> PointsOfInterest { get; set; } = new List<PointOfInterest>();
    }
}