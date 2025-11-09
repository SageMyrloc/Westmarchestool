namespace Westmarchestool.Core.Entities.HexMap
{
    /// <summary>
    /// Junction table for Expedition membership.
    /// </summary>
    public class ExpeditionMember
    {
        public int Id { get; set; }

        // Foreign keys
        public int ExpeditionId { get; set; }
        public Expedition Expedition { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Membership dates
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeftDate { get; set; }

        // Did this member push their map to Town Map when leaving?
        public bool PushedToTownMap { get; set; } = false;
        public DateTime? PushDate { get; set; }
    }
}