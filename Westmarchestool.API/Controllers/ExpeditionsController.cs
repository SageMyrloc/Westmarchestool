using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Westmarchestool.Infrastructure.Data;
using Westmarchestool.HexMap.Entities;
using Westmarchestool.HexMap.Coordinates;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpeditionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpeditionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all expeditions
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult<List<Expedition>>> GetAllExpeditions()
        {
            var expeditions = await _context.Expeditions
                .Include(e => e.LeaderPlayer)
                .Include(e => e.Members)
                .OrderByDescending(e => e.DepartureTime)
                .ToListAsync();

            return Ok(expeditions);
        }

        /// <summary>
        /// Get active expeditions only
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<Expedition>>> GetActiveExpeditions()
        {
            var expeditions = await _context.Expeditions
                .Include(e => e.LeaderPlayer)
                .Include(e => e.Members)
                .Where(e => e.Status == "Active")
                .ToListAsync();

            return Ok(expeditions);
        }

        /// <summary>
        /// Get a specific expedition
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Expedition>> GetExpedition(int id)
        {
            var expedition = await _context.Expeditions
                .Include(e => e.LeaderPlayer)
                .Include(e => e.Members)
                .Include(e => e.ExploredHexes)
                .FirstOrDefaultAsync(e => e.Id == id);

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
            var expedition = new Expedition
            {
                GroupName = dto.GroupName,
                LeaderPlayerId = dto.LeaderPlayerId,
                DepartureTime = DateTime.UtcNow,
                StartQ = dto.StartQ,
                StartR = dto.StartR,
                LastKnownQ = dto.StartQ,
                LastKnownR = dto.StartR,
                Status = "Active"
            };

            _context.Expeditions.Add(expedition);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExpedition), new { id = expedition.Id }, expedition);
        }

        /// <summary>
        /// Add a member to an expedition
        /// </summary>
        [HttpPost("{id}/members")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
        {
            var expedition = await _context.Expeditions.FindAsync(id);
            if (expedition == null)
            {
                return NotFound(new { message = "Expedition not found" });
            }

            // Check if character already in expedition
            var existing = await _context.ExpeditionMembers
                .FirstOrDefaultAsync(em => em.ExpeditionId == id && em.CharacterId == dto.CharacterId);

            if (existing != null)
            {
                return BadRequest(new { message = "Character already in expedition" });
            }

            var member = new ExpeditionMember
            {
                ExpeditionId = id,
                CharacterId = dto.CharacterId,
                PlayerId = dto.PlayerId
            };

            _context.ExpeditionMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Member added successfully" });
        }

        /// <summary>
        /// Record exploration of a hex
        /// </summary>
        [HttpPost("{id}/explore")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult> RecordExploration(int id, [FromBody] RecordExplorationDto dto)
        {
            var expedition = await _context.Expeditions.FindAsync(id);
            if (expedition == null)
            {
                return NotFound(new { message = "Expedition not found" });
            }

            var exploredHex = new ExpeditionHex
            {
                ExpeditionId = id,
                Q = dto.BelievedQ,
                R = dto.BelievedR,
                ActualQ = dto.ActualQ,
                ActualR = dto.ActualR,
                TerrainType = dto.TerrainType,
                IsAccurate = (dto.BelievedQ == dto.ActualQ && dto.BelievedR == dto.ActualR)
            };

            _context.ExpeditionHexes.Add(exploredHex);

            // Update expedition's last known position
            expedition.LastKnownQ = dto.ActualQ;
            expedition.LastKnownR = dto.ActualR;

            await _context.SaveChangesAsync();

            return Ok(exploredHex);
        }

        /// <summary>
        /// Complete an expedition (return to town)
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,GM")]
        public async Task<ActionResult> CompleteExpedition(int id)
        {
            var expedition = await _context.Expeditions
                .Include(e => e.ExploredHexes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expedition == null)
            {
                return NotFound(new { message = "Expedition not found" });
            }

            expedition.ReturnTime = DateTime.UtcNow;
            expedition.Status = "Returned";

            // TODO: Implement synchronization to Town Map
            // For now, just mark expedition as complete

            await _context.SaveChangesAsync();

            return Ok(new { message = "Expedition completed successfully" });
        }

        /// <summary>
        /// Get the expedition's explored hexes
        /// </summary>
        [HttpGet("{id}/map")]
        public async Task<ActionResult<List<ExpeditionHex>>> GetExpeditionMap(int id)
        {
            var hexes = await _context.ExpeditionHexes
                .Where(eh => eh.ExpeditionId == id)
                .OrderBy(eh => eh.ExploredTime)
                .ToListAsync();

            return Ok(hexes);
        }
    }

    // DTOs for expedition operations
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
}