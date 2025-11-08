namespace Westmarchestool.Core.Entities
{
    public class CharacterJsonData
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        // Store the full Pathbuilder JSON
        public string JsonContent { get; set; } = string.Empty;

        // Metadata
        public DateTime ImportedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    }
}