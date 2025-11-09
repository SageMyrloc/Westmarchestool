using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.Core.Entities.HexMap
{
    /// <summary>
    /// Represents a Point of Interest (POI) like ruins, caves, landmarks, etc.
    /// Has both true location (GM Map) and player-believed location (Town Map).
    /// </summary>
    public class PointOfInterest
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        // TRUE location (on GM Map)
        [Required]
        public int TrueQ { get; set; }

        [Required]
        public int TrueR { get; set; }

        // Foreign key to GM Map hex
        public int? GMMapHexId { get; set; }
        public GMMapHex? GMMapHex { get; set; }

        // Where players THINK it is (on Town Map)
        public int? PlayerKnownQ { get; set; }
        public int? PlayerKnownR { get; set; }

        // Foreign key to Town Map hex (nullable until discovered)
        public int? TownMapHexId { get; set; }
        public TownMapHex? TownMapHex { get; set; }

        // Discovery metadata
        public int? DiscoveredByExpeditionId { get; set; }
        public Expedition? DiscoveredByExpedition { get; set; }

        public DateTime? DiscoveryDate { get; set; }

        // Is the location verified as correct?
        public bool IsLocationVerified { get; set; } = false;

        // Optional link to lore article (future feature)
        [MaxLength(500)]
        public string? LoreArticleUrl { get; set; }

        // Dates
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}