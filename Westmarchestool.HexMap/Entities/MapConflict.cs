using System.ComponentModel.DataAnnotations;
using Westmarchestool.Core.Entities;

namespace Westmarchestool.HexMap.Entities
{
    /// <summary>
    /// Represents a conflict between expedition data and existing town map data
    /// </summary>
    public class MapConflict
    {
        public int Id { get; set; }

        // Location of conflict
        public int Q { get; set; }
        public int R { get; set; }

        // The new submission that conflicts
        public int SubmissionId { get; set; }
        public TownMapSubmission Submission { get; set; } = null!;

        // Who submitted the NEW data
        public int NewSubmitterId { get; set; }
        public User NewSubmitter { get; set; } = null!;

        // Who submitted the EXISTING data
        public int? ExistingSubmitterId { get; set; }
        public User? ExistingSubmitter { get; set; }

        // The conflicting data
        [Required]
        [MaxLength(50)]
        public string NewTerrainType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ExistingTerrainType { get; set; }

        // Status
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Unresolved"; // Unresolved, Voting, Resolved

        // Resolution
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedDate { get; set; }

        [MaxLength(20)]
        public string? Resolution { get; set; } // AcceptNew, KeepExisting, Verification, GMOverride

        public string? ResolutionNotes { get; set; }

        // Navigation
        public ICollection<ConflictVote> Votes { get; set; } = new List<ConflictVote>();
    }
}