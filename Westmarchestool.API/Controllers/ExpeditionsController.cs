using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Westmarchestool.HexMap.Services;
using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.Core.Entities.HexMap;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpeditionsController : ControllerBase
    {
        private readonly IExpeditionService _expeditionService;
        private readonly IMapMergeService _mapMergeService;

        public ExpeditionsController(IExpeditionService expeditionService, IMapMergeService mapMergeService)
        {
            _expeditionService = expeditionService;
            _mapMergeService = mapMergeService;
        }

        // GET: api/Expeditions/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveExpeditions()
        {
            var expeditions = await _expeditionService.GetActiveExpeditionsAsync();
            return Ok(expeditions);
        }

        // GET: api/Expeditions/my-expedition
        [HttpGet("my-expedition")]
        public async Task<IActionResult> GetMyActiveExpedition()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var expedition = await _expeditionService.GetUserActiveExpeditionAsync(userId.Value);

            if (expedition == null)
            {
                return Ok(new { hasExpedition = false });
            }

            return Ok(new { hasExpedition = true, expedition });
        }

        // GET: api/Expeditions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpedition(int id)
        {
            var expedition = await _expeditionService.GetExpeditionByIdAsync(id);

            if (expedition == null)
            {
                return NotFound(new { message = "Expedition not found" });
            }

            return Ok(expedition);
        }

        // GET: api/Expeditions/{id}/hexes
        [HttpGet("{id}/hexes")]
        public async Task<IActionResult> GetExpeditionHexes(int id)
        {
            var hexes = await _expeditionService.GetExpeditionHexesAsync(id);
            return Ok(hexes);
        }

        // POST: api/Expeditions/{id}/join
        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinExpedition(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _expeditionService.JoinExpeditionAsync(id, userId.Value);

            if (!result)
            {
                return BadRequest(new { message = "Cannot join expedition. You may already be in an active expedition, or the expedition is not active." });
            }

            return Ok(new { message = "Successfully joined expedition" });
        }

        // POST: api/Expeditions/{id}/leave
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveExpedition(int id, [FromBody] LeaveExpeditionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _expeditionService.LeaveExpeditionAsync(id, userId.Value, request.PushToTownMap);

            if (!result)
            {
                return BadRequest(new { message = "Cannot leave expedition. You may not be a member." });
            }

            // If player chose to push their map, trigger merge
            if (request.PushToTownMap)
            {
                var mergeResult = await _mapMergeService.MergePlayerMapToTownMapAsync(id, userId.Value);

                return Ok(new
                {
                    message = "Left expedition and pushed map to Town Map",
                    mergeResult
                });
            }

            return Ok(new { message = "Successfully left expedition" });
        }

        // POST: api/Expeditions/{id}/mark-hex
        [HttpPost("{id}/mark-hex")]
        public async Task<IActionResult> MarkHex(int id, [FromBody] MarkHexRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var coordinate = new HexCoordinate(request.Q, request.R);
            var result = await _expeditionService.MarkHexAsync(id, userId.Value, coordinate, request.Terrain, request.Notes);

            if (!result)
            {
                return BadRequest(new { message = "Cannot mark hex. You may not be the expedition leader, or the expedition is not active." });
            }

            return Ok(new { message = "Hex marked successfully" });
        }

        // POST: api/Expeditions/{id}/reassign-leader
        [HttpPost("{id}/reassign-leader")]
        public async Task<IActionResult> ReassignLeader(int id, [FromBody] ReassignLeaderRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _expeditionService.ReassignLeaderAsync(id, request.NewLeaderUserId, userId.Value);

            if (!result)
            {
                return BadRequest(new { message = "Cannot reassign leader. The new leader must be a current member." });
            }

            return Ok(new { message = "Leader reassigned successfully" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;
            return int.Parse(userIdClaim.Value);
        }
    }

    // DTOs
    public class LeaveExpeditionRequest
    {
        public bool PushToTownMap { get; set; }
    }

    public class MarkHexRequest
    {
        public int Q { get; set; }
        public int R { get; set; }
        public TerrainType Terrain { get; set; }
        public string? Notes { get; set; }
    }

    public class ReassignLeaderRequest
    {
        public int NewLeaderUserId { get; set; }
    }
}