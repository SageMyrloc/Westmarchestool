using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.API.Models
{
    public enum CharacterStatus
    {
        Pending,      // Awaiting admin approval
        Active,       // Approved and playable
        Township,     // Retired but alive
        Graveyard     // Dead
    }

    public class Character
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Searchable fields extracted from JSON
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Class { get; set; } = string.Empty;

        public int Level { get; set; } = 1;

        [MaxLength(50)]
        public string Ancestry { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Background { get; set; } = string.Empty;

        public int Experience { get; set; } = 0;

        // Money (stored separately for easy modification)
        public int CopperPieces { get; set; } = 0;
        public int SilverPieces { get; set; } = 0;
        public int GoldPieces { get; set; } = 0;
        public int PlatinumPieces { get; set; } = 0;

        // Character portrait
        [MaxLength(500)]
        public string? PortraitUrl { get; set; }

        // Status
        public CharacterStatus Status { get; set; } = CharacterStatus.Pending;

        // Dates
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedByUserId { get; set; }

        // Navigation properties
        public CharacterJsonData? JsonData { get; set; }
        public ICollection<CharacterInventoryItem> Inventory { get; set; } = new List<CharacterInventoryItem>();
        public ICollection<SessionAttendee> SessionAttendances { get; set; } = new List<SessionAttendee>();
        public ICollection<CharacterHistoryEntry> History { get; set; } = new List<CharacterHistoryEntry>();
    }
}