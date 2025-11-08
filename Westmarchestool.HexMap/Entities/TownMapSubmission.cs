using System.ComponentModel.DataAnnotations;
using Westmarchestool.Core.Entities;

namespace Westmarchestool.HexMap.Entities
{
    /// <summary>
    /// Tracks when expeditions submit their maps to the Town Map
    /// </summary>
    public class TownMapSubmission
    {
        public int Id { get; set; }

        // Which expedition submitted
        public int ExpeditionId { get; set; }
        public Expedition Expedition { get; set; } = null!;

        // Who submitted (expedition leader)
        public int SubmittedByPlayerId { get; set; }
        public User SubmittedByPlayer { get; set; } = null!;

        // When submitted
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        // Status
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, HasConflicts

        // How many hexes were submitted
        public int HexesSubmitted { get; set; }
        public int HexesAccepted { get; set; }
        public int HexesConflicted { get; set; }

        // Notes
        public string? Notes { get; set; }
    }
}