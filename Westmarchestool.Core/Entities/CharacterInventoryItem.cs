using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.Core.Entities
{
    public enum InventoryItemSource
    {
        Shop,           // Purchased from shop
        Loot,           // Given by GM as loot
        Trade,          // Traded with another player
        Session,        // Reward from session
        Other           // Other source
    }

    public class CharacterInventoryItem
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        // Optional: Store item stats as JSON if needed
        public string? ItemDataJson { get; set; }

        public InventoryItemSource Source { get; set; }

        public DateTime AcquiredDate { get; set; } = DateTime.UtcNow;

        // If from shop, which shop/transaction
        public int? ShopTransactionId { get; set; }
    }
}