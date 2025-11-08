using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.HexMap.Entities
{
    /// <summary>
    /// Represents a hex as explored by a specific expedition
    /// Temporary data - deleted after expedition returns to town
    /// </summary>
    public class ExpeditionHex
    {
        public int Id { get; set; }

        // Which expedition explored this
        public int ExpeditionId { get; set; }
        public Expedition Expedition { get; set; } = null!;

        // Where the group BELIEVES they are
        public int Q { get; set; }
        public int R { get; set; }

        // Where they ACTUALLY are (may differ if lost)
        public int ActualQ { get; set; }
        public int ActualR { get; set; }

        // What they observed
        [Required]
        [MaxLength(50)]
        public string TerrainType { get; set; } = string.Empty;

        // Accuracy flag
        public bool IsAccurate { get; set; } = true;  // Q,R matches ActualQ,ActualR?

        // When explored
        public DateTime ExploredTime { get; set; } = DateTime.UtcNow;
    }
}