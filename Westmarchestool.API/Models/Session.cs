using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.API.Models
{
    public enum SessionStatus
    {
        Scheduled,    // Not yet played
        InProgress,   // Currently happening
        Completed,    // Finished, rewards distributed
        Cancelled     // Cancelled
    }

    public class Session
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int GameMasterUserId { get; set; }
        public User GameMaster { get; set; } = null!;

        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

        // Rewards (applied to all attendees)
        public int GoldReward { get; set; } = 0;
        public int ExperienceReward { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<SessionAttendee> Attendees { get; set; } = new List<SessionAttendee>();
    }
}