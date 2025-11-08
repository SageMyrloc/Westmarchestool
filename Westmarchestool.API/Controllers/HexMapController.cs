using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Westmarchestool.HexMap.Entities;
using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.HexMap.Services;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,GM")]
    public class HexMapController : ControllerBase
    {
        private readonly IHexMapService _hexMapService;

        public HexMapController(IHexMapService hexMapService)
        {
            _hexMapService = hexMapService;
        }

        /// <summary>
        /// Get a specific hex by coordinates
        /// </summary>
        [HttpGet("{q}/{r}")]
        public async Task<ActionResult<HexTile>> GetHex(int q, int r)
        {
            var hex = await _hexMapService.GetHexAsync(q, r);

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
            var hexes = await _hexMapService.GetGMMapAsync();
            return Ok(hexes);
        }

        /// <summary>
        /// Get all hexes on the Town Map (public)
        /// </summary>
        [HttpGet("town-map")]
        [AllowAnonymous]
        public async Task<ActionResult<List<HexTile>>> GetTownMap()
        {
            var hexes = await _hexMapService.GetTownMapAsync();
            return Ok(hexes);
        }

        /// <summary>
        /// Create a new hex
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<HexTile>> CreateHex([FromBody] CreateHexDto dto)
        {
            try
            {
                var position = new AxialHex(dto.Q, dto.R);
                var hex = await _hexMapService.CreateHexAsync(position, dto.TerrainType);

                // Apply optional settings
                if (dto.IsPublic)
                {
                    await _hexMapService.MarkHexAsPublicAsync(hex.Q, hex.R);
                }

                // Refresh to get updated hex
                hex = await _hexMapService.GetHexAsync(hex.Q, hex.R);

                return CreatedAtAction(nameof(GetHex), new { q = hex!.Q, r = hex.R }, hex);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing hex
        /// </summary>
        [HttpPut("{q}/{r}")]
        public async Task<ActionResult<HexTile>> UpdateHex(int q, int r, [FromBody] UpdateHexDto dto)
        {
            try
            {
                var hex = await _hexMapService.UpdateHexAsync(q, r, dto.TerrainType);

                if (dto.IsPublic.HasValue && dto.IsPublic.Value)
                {
                    await _hexMapService.MarkHexAsPublicAsync(q, r);
                }

                return Ok(hex);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all neighbors of a hex
        /// </summary>
        [HttpGet("{q}/{r}/neighbors")]
        public async Task<ActionResult<List<HexTile>>> GetNeighbors(int q, int r)
        {
            var axial = new AxialHex(q, r);
            var neighbors = await _hexMapService.GetNeighborsAsync(axial);
            return Ok(neighbors);
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
        /// Generate border hexes around a position
        /// </summary>
        [HttpPost("generate-border")]
        public async Task<ActionResult> GenerateBorder([FromBody] GenerateBorderDto dto)
        {
            try
            {
                var center = new AxialHex(dto.CenterQ, dto.CenterR);
                await _hexMapService.GenerateBorderHexesAsync(center, dto.BorderDistance);

                return Ok(new { message = $"Generated hexes within {dto.BorderDistance} of ({dto.CenterQ}, {dto.CenterR})" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs
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

    public class GenerateBorderDto
    {
        public int CenterQ { get; set; }
        public int CenterR { get; set; }
        public int BorderDistance { get; set; } = 2;
    }
}