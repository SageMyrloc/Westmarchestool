using Westmarchestool.Core.Entities;

namespace Westmarchestool.HexMap.Entities
{
    /// <summary>
    /// Player votes on map conflicts
    /// </summary>
    public class ConflictVote
    {
        public int Id { get; set; }

        // Which conflict
        public int ConflictId { get; set; }
        public MapConflict Conflict { get; set; } = null!;

        // Who voted
        public int PlayerId { get; set; }
        public User Player { get; set; } = null!;

        // Their vote
        public bool VoteForNew { get; set; } // true = accept new data, false = keep existing

        // When they voted
        public DateTime VotedDate { get; set; } = DateTime.UtcNow;

        // Optional comment
        public string? Comment { get; set; }
    }
}