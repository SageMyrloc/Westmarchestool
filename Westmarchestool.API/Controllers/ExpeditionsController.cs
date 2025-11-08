using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.HexMap.Entities;
using Westmarchestool.HexMap.Services;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpeditionsController : ControllerBase
    {
        private readonly IExpeditionService _expeditionService;

        public ExpeditionsController(IExpeditionService expeditionService)
        {
            _expeditionService = expeditionService;
        }

        /// <summary>
        /// Get active expeditions
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<Expedition>>> GetActiveExpeditions()
        {
            var expeditions = await _expeditionService.GetActiveExpeditionsAsync();
            return Ok(expeditions);
        }

        /// <summary>
        /// Get a specific expedition
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Expedition>> GetExpedition(int id)
        {
            var expedition = await _expeditionService.GetExpeditionAsync(id);

            if (expedition == null)
            {
                return NotFound(new { message = "Expedition not found" });
            }

            return Ok(expedition);
        }

        /// <summary>
        /// Create a new expedition
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult<Expedition>> CreateExpedition([FromBody] CreateExpeditionDto dto)
        {
            try
            {
                var startPosition = new AxialHex(dto.StartQ, dto.StartR);
                var expedition = await _expeditionService.CreateExpeditionAsync(
                    dto.GroupName,
                    dto.LeaderPlayerId,
                    startPosition
                );

                return CreatedAtAction(nameof(GetExpedition), new { id = expedition.Id }, expedition);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add a member to an expedition
        /// </summary>
        [HttpPost("{id}/members")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
        {
            try
            {
                await _expeditionService.AddMemberAsync(id, dto.CharacterId, dto.PlayerId);
                return Ok(new { message = "Member added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Record exploration of a hex
        /// </summary>
        [HttpPost("{id}/explore")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult<ExpeditionHex>> RecordExploration(int id, [FromBody] RecordExplorationDto dto)
        {
            try
            {
                var believedPosition = new AxialHex(dto.BelievedQ, dto.BelievedR);
                var actualPosition = new AxialHex(dto.ActualQ, dto.ActualR);

                var exploredHex = await _expeditionService.RecordExplorationAsync(
                    id,
                    believedPosition,
                    actualPosition,
                    dto.TerrainType
                );

                return Ok(exploredHex);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Complete an expedition (marks as returned, does NOT auto-sync)
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult<Expedition>> CompleteExpedition(int id)
        {
            try
            {
                var expedition = await _expeditionService.CompleteExpeditionAsync(id);
                return Ok(new
                {
                    expedition = expedition,
                    message = "Expedition marked as returned. Use /submit endpoint to submit map to town."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get the expedition's explored hexes
        /// </summary>
        [HttpGet("{id}/map")]
        public async Task<ActionResult<List<ExpeditionHex>>> GetExpeditionMap(int id)
        {
            var hexes = await _expeditionService.GetExpeditionMapAsync(id);
            return Ok(hexes);
        }

        /// <summary>
        /// Check if expedition is lost
        /// </summary>
        [HttpGet("{id}/is-lost")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult<bool>> IsExpeditionLost(int id)
        {
            var isLost = await _expeditionService.IsExpeditionLostAsync(id);
            return Ok(new { expeditionId = id, isLost = isLost });
        }

        /// <summary>
        /// Submit expedition map to town (PLAYER-DRIVEN!)
        /// </summary>
        [HttpPost("{id}/submit")]
        [Authorize]
        public async Task<ActionResult<TownMapSubmission>> SubmitToTown(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                int playerId = int.Parse(userIdClaim.Value);

                var submission = await _expeditionService.SubmitExpeditionToTownAsync(id, playerId);

                return Ok(new
                {
                    submission = submission,
                    message = submission.HexesConflicted > 0
                        ? $"Submitted with {submission.HexesConflicted} conflict(s) requiring resolution"
                        : "All hexes accepted!"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all pending submissions
        /// </summary>
        [HttpGet("submissions/pending")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult<List<TownMapSubmission>>> GetPendingSubmissions()
        {
            var submissions = await _expeditionService.GetPendingSubmissionsAsync();
            return Ok(submissions);
        }

        /// <summary>
        /// Get a specific submission
        /// </summary>
        [HttpGet("submissions/{id}")]
        [Authorize]
        public async Task<ActionResult<TownMapSubmission>> GetSubmission(int id)
        {
            var submission = await _expeditionService.GetSubmissionAsync(id);
            if (submission == null)
            {
                return NotFound(new { message = "Submission not found" });
            }
            return Ok(submission);
        }

        /// <summary>
        /// Get all unresolved conflicts
        /// </summary>
        [HttpGet("conflicts")]
        [Authorize]
        public async Task<ActionResult<List<MapConflict>>> GetConflicts()
        {
            var conflicts = await _expeditionService.GetUnresolvedConflictsAsync();
            return Ok(conflicts);
        }

        /// <summary>
        /// Get a specific conflict
        /// </summary>
        [HttpGet("conflicts/{id}")]
        [Authorize]
        public async Task<ActionResult<MapConflict>> GetConflict(int id)
        {
            var conflict = await _expeditionService.GetConflictAsync(id);
            if (conflict == null)
            {
                return NotFound(new { message = "Conflict not found" });
            }
            return Ok(conflict);
        }

        /// <summary>
        /// Vote on a conflict
        /// </summary>
        [HttpPost("conflicts/{id}/vote")]
        [Authorize]
        public async Task<ActionResult> VoteOnConflict(int id, [FromBody] VoteDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                int playerId = int.Parse(userIdClaim.Value);

                await _expeditionService.VoteOnConflictAsync(id, playerId, dto.VoteForNew, dto.Comment);

                var tally = await _expeditionService.GetConflictVoteTallyAsync(id);

                return Ok(new
                {
                    message = "Vote recorded",
                    tally = new
                    {
                        votesForNew = tally[true],
                        votesForExisting = tally[false]
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Resolve a conflict (GM/Admin only)
        /// </summary>
        [HttpPost("conflicts/{id}/resolve")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult> ResolveConflict(int id, [FromBody] ResolveConflictDto dto)
        {
            try
            {
                await _expeditionService.ResolveConflictAsync(id, dto.Resolution, dto.Notes);
                return Ok(new { message = "Conflict resolved" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get vote tally for a conflict
        /// </summary>
        [HttpGet("conflicts/{id}/votes")]
        [Authorize]
        public async Task<ActionResult> GetVoteTally(int id)
        {
            var tally = await _expeditionService.GetConflictVoteTallyAsync(id);
            return Ok(new
            {
                conflictId = id,
                votesForNew = tally[true],
                votesForExisting = tally[false],
                total = tally[true] + tally[false]
            });
        }
    }

    // DTOs
    public class CreateExpeditionDto
    {
        public string GroupName { get; set; } = string.Empty;
        public int LeaderPlayerId { get; set; }
        public int StartQ { get; set; }
        public int StartR { get; set; }
    }

    public class AddMemberDto
    {
        public int CharacterId { get; set; }
        public int PlayerId { get; set; }
    }

    public class RecordExplorationDto
    {
        public int BelievedQ { get; set; }
        public int BelievedR { get; set; }
        public int ActualQ { get; set; }
        public int ActualR { get; set; }
        public string TerrainType { get; set; } = string.Empty;
    }

    public class VoteDto
    {
        public bool VoteForNew { get; set; }
        public string? Comment { get; set; }
    }

    public class ResolveConflictDto
    {
        public string Resolution { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}