using Westmarchestool.API.Models;

namespace Westmarchestool.API.DTOs
{
    public class CharacterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Ancestry { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public int Experience { get; set; }
        public MoneyDto Money { get; set; } = new();
        public string? PortraitUrl { get; set; }
        public CharacterStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
    }

    public class MoneyDto
    {
        public int Cp { get; set; }
        public int Sp { get; set; }
        public int Gp { get; set; }
        public int Pp { get; set; }
    }
}