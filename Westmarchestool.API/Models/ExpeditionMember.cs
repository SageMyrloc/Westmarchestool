namespace Westmarchestool.API.Models
{
    /// <summary>
    /// Tracks which characters participated in which expedition
    /// </summary>
    public class ExpeditionMember
    {
        public int Id { get; set; }

        // Which expedition
        public int ExpeditionId { get; set; }
        public Expedition Expedition { get; set; } = null!;

        // Which character
        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        // Which player controls this character
        public int PlayerId { get; set; }
        public User Player { get; set; } = null!;

        // Timing
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }  // If left early (death, separation)
    }
}