using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Westmarchestool.API.DTOs
{
    public class ImportCharacterDto
    {
        [Required]
        public JsonElement PathbuilderJson { get; set; }  // Accept as JSON object

        public string? PortraitUrl { get; set; }
    }
}