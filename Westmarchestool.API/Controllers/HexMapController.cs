using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Westmarchestool.Infrastructure.Data;
using Westmarchestool.HexMap.Entities;
using Westmarchestool.HexMap.Coordinates;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,GM")]  // Only GMs and Admins can access
    public class HexMapController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HexMapController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a specific hex by coordinates
        /// </summary>
        [HttpGet("{q}/{r}")]
        public async Task<ActionResult<HexTile>> GetHex(int q, int r)
        {
            var hex = await _context.HexTiles.FindAsync(q, r);

            if (hex == null)
            {
                return NotFound(new { message = $"Hex at ({q}, {r}) not found" });
            }

            return Ok(hex);
        }

        /// <summary>
        /// Get all hexes on the GM Master Map
        /// </summary>
        [HttpGet("gm-map")]
        public async Task<ActionResult<List<HexTile>>> GetGMMap()
        {
            var hexes = await _context.HexTiles
                .Where(h => h.IsExploredByGM)
                .ToListAsync();

            return Ok(hexes);
        }

        /// <summary>
        /// Get all hexes on the Town Map (public)
        /// </summary>
        [HttpGet("town-map")]
        [AllowAnonymous]  // Players can see this
        public async Task<ActionResult<List<HexTile>>> GetTownMap()
        {
            var hexes = await _context.HexTiles
                .Where(h => h.IsOnTownMap)
                .ToListAsync();

            return Ok(hexes);
        }

        /// <summary>
        /// Create a new hex
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<HexTile>> CreateHex([FromBody] CreateHexDto dto)
        {
            // Check if hex already exists
            var existing = await _context.HexTiles.FindAsync(dto.Q, dto.R);
            if (existing != null)
            {
                return BadRequest(new { message = $"Hex at ({dto.Q}, {dto.R}) already exists" });
            }

            var hex = new HexTile
            {
                Q = dto.Q,
                R = dto.R,
                TerrainType = dto.TerrainType,
                IsExploredByGM = true,
                IsOnTownMap = dto.IsPublic,
                GMNotes = dto.GMNotes
            };

            _context.HexTiles.Add(hex);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHex), new { q = hex.Q, r = hex.R }, hex);
        }

        /// <summary>
        /// Update an existing hex
        /// </summary>
        [HttpPut("{q}/{r}")]
        public async Task<ActionResult<HexTile>> UpdateHex(int q, int r, [FromBody] UpdateHexDto dto)
        {
            var hex = await _context.HexTiles.FindAsync(q, r);

            if (hex == null)
            {
                return NotFound(new { message = $"Hex at ({q}, {r}) not found" });
            }

            if (!string.IsNullOrEmpty(dto.TerrainType))
                hex.TerrainType = dto.TerrainType;

            if (dto.IsPublic.HasValue)
                hex.IsOnTownMap = dto.IsPublic.Value;

            if (dto.GMNotes != null)
                hex.GMNotes = dto.GMNotes;

            hex.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(hex);
        }

        /// <summary>
        /// Get all neighbors of a hex
        /// </summary>
        [HttpGet("{q}/{r}/neighbors")]
        public async Task<ActionResult<List<HexTile>>> GetNeighbors(int q, int r)
        {
            var axial = new AxialHex(q, r);
            var neighbors = axial.GetAllNeighbors();

            var hexNeighbors = new List<HexTile>();

            foreach (var neighbor in neighbors)
            {
                var hex = await _context.HexTiles.FindAsync(neighbor.Q, neighbor.R);
                if (hex != null)
                {
                    hexNeighbors.Add(hex);
                }
            }

            return Ok(hexNeighbors);
        }

        /// <summary>
        /// Calculate distance between two hexes
        /// </summary>
        [HttpGet("distance")]
        public ActionResult<int> GetDistance(int q1, int r1, int q2, int r2)
        {
            var hex1 = new AxialHex(q1, r1);
            var hex2 = new AxialHex(q2, r2);

            int distance = HexMath.Distance(hex1, hex2);

            return Ok(new
            {
                from = new { q = q1, r = r1 },
                to = new { q = q2, r = r2 },
                distance = distance
            });
        }

        /// <summary>
        /// Delete a hex (GM only)
        /// </summary>
        [HttpDelete("{q}/{r}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteHex(int q, int r)
        {
            var hex = await _context.HexTiles.FindAsync(q, r);

            if (hex == null)
            {
                return NotFound(new { message = $"Hex at ({q}, {r}) not found" });
            }

            _context.HexTiles.Remove(hex);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hex deleted successfully" });
        }
    }

    // DTOs for hex operations
    public class CreateHexDto
    {
        public int Q { get; set; }
        public int R { get; set; }
        public string TerrainType { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
        public string? GMNotes { get; set; }
    }

    public class UpdateHexDto
    {
        public string? TerrainType { get; set; }
        public bool? IsPublic { get; set; }
        public string? GMNotes { get; set; }
    }
}