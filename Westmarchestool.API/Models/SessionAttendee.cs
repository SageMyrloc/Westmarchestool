namespace Westmarchestool.API.Models
{
    public class SessionAttendee
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; } = null!;

        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public DateTime SignedUpDate { get; set; } = DateTime.UtcNow;

        // Track if player actually attended
        public bool Attended { get; set; } = true;

        // Rewards received (copied from session when distributed)
        public int GoldReceived { get; set; } = 0;
        public int ExperienceReceived { get; set; } = 0;
        public DateTime? RewardsDistributedDate { get; set; }
    }
}