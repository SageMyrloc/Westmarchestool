using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.API.Models
{
    public enum HistoryChangeType
    {
        Created,
        Imported,
        Approved,
        FieldChanged,
        StatusChanged,
        LevelUp,
        ItemAdded,
        ItemRemoved,
        SessionReward
    }

    public class CharacterHistoryEntry
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public HistoryChangeType ChangeType { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        // Who made the change
        public int? ChangedByUserId { get; set; }
        public User? ChangedByUser { get; set; }

        // Old and new values (for field changes)
        [MaxLength(500)]
        public string? OldValue { get; set; }

        [MaxLength(500)]
        public string? NewValue { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    }
}