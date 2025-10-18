using System.ComponentModel.DataAnnotations;

namespace Westmarchestool.API.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}