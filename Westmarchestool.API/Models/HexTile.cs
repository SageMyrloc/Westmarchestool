using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace Westmarchestool.API.Models
{
    /// Represents a single hex tile in the world
    /// Stores both GM Master Map and Town Map data
    public class HexTile
    {
        // Composite primary key (Q, R)
        [Key]
        public int Q { get; set; }

        [Key]
        public int R { get; set; }

        // Terrain information
        [Required]
        [MaxLength(50)]
        public string TerrainType { get; set; } = string.Empty;

        // Map visibility flags
        public bool IsExploredByGM { get; set; } = false;  // GM Master Map
        public bool IsOnTownMap { get; set; } = false;     // Town Map (players can see)

        // Discovery tracking
        public int? DiscoveredByExpeditionId { get; set; }
        public DateTime? DiscoveredDate { get; set; }

        // Accuracy tracking (for when Town Map is wrong)
        public bool IsAccurate { get; set; } = true;
        public int? ActualQ { get; set; }  // If inaccurate, links to correct hex
        public int? ActualR { get; set; }

        // Additional data
        public string? GMNotes { get; set; }  // Private GM notes
        public bool HasPOI { get; set; } = false;  // Point of Interest flag
        public string? POIData { get; set; }  // JSON for POI details

        // Timestamps
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property (will connect to Expedition later)
        public Expedition? DiscoveredByExpedition { get; set; }
    }
}
