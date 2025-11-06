using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.API.Models
{
    /// <summary>
    /// Represents a group expedition (active or completed)
    /// </summary>
    public class Expedition
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; } = string.Empty;

        // Who leads this expedition
        public int LeaderPlayerId { get; set; }
        public User LeaderPlayer { get; set; } = null!;

        // Timing
        public DateTime DepartureTime { get; set; }
        public DateTime? ReturnTime { get; set; }  // Null if still active

        // Starting position (always accurate)
        public int StartQ { get; set; }
        public int StartR { get; set; }

        // Last known position
        public int? LastKnownQ { get; set; }
        public int? LastKnownR { get; set; }

        // Status tracking
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";  // Active, Returned, Lost, TPK

        // Notes
        public string? Notes { get; set; }

        // Navigation properties
        public ICollection<ExpeditionHex> ExploredHexes { get; set; } = new List<ExpeditionHex>();
        public ICollection<ExpeditionMember> Members { get; set; } = new List<ExpeditionMember>();
    }
}
