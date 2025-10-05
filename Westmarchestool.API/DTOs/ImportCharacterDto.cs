using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.API.DTOs
{
    public class ImportCharacterDto
    {
        [Required]
        public string PathbuilderJson { get; set; } = string.Empty;

        public string? PortraitUrl { get; set; }
    }
}