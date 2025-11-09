using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.Core.Entities.HexMap
{
    /// <summary>
    /// Represents an exploration expedition with its own temporary map.
    /// </summary>
    public class Expedition
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Expedition leader (FK to User)
        [Required]
        public int LeaderUserId { get; set; }
        public User Leader { get; set; } = null!;

        // Starting hex coordinates
        public int StartQ { get; set; }
        public int StartR { get; set; }

        // Status
        [Required]
        public ExpeditionStatus Status { get; set; } = ExpeditionStatus.Active;

        // Dates
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        // Navigation properties
        public ICollection<ExpeditionMember> Members { get; set; } = new List<ExpeditionMember>();
        public ICollection<ExpeditionHex> Hexes { get; set; } = new List<ExpeditionHex>();
    }
}