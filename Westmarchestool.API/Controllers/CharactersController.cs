using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Westmarchestool.API.DTOs;
using Westmarchestool.API.Models;
using Westmarchestool.API.Services;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CharactersController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharactersController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportCharacter([FromBody] ImportCharacterDto importDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var character = await _characterService.ImportCharacterAsync(userId.Value, importDto);

            if (character == null)
            {
                return BadRequest(new { message = "Failed to import character. Check JSON format." });
            }

            return Ok(character);
        }

        [HttpGet("my-characters")]
        public async Task<IActionResult> GetMyCharacters()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var characters = await _characterService.GetUserCharactersAsync(userId.Value);
            return Ok(characters);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCharacter(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var character = await _characterService.GetCharacterByIdAsync(id, userId.Value);

            if (character == null)
            {
                return NotFound(new { message = "Character not found or access denied" });
            }

            return Ok(character);
        }

        [HttpGet("{id}/json")]
        public async Task<IActionResult> GetCharacterJson(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var json = await _characterService.GetCharacterJsonAsync(id, userId.Value);

            if (json == null)
            {
                return NotFound(new { message = "Character not found or access denied" });
            }

            return Content(json, "application/json");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingCharacters()
        {
            var characters = await _characterService.GetPendingCharactersAsync();
            return Ok(characters);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveCharacter(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _characterService.ApproveCharacterAsync(id, userId.Value);

            if (!result)
            {
                return NotFound(new { message = "Character not found or already approved" });
            }

            return Ok(new { message = "Character approved successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateCharacterStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _characterService.UpdateCharacterStatusAsync(id, dto.Status, userId.Value);

            if (!result)
            {
                return NotFound(new { message = "Character not found" });
            }

            return Ok(new { message = "Character status updated successfully" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;
            return int.Parse(userIdClaim.Value);
        }
    }

    public class UpdateStatusDto
    {
        public CharacterStatus Status { get; set; }
    }
}