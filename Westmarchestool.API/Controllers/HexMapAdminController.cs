using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Westmarchestool.HexMap.Services;
using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.HexMap.Generation;
using Westmarchestool.Core.Entities.HexMap;
using Westmarchestool.Infrastructure.Data;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,GM")]
    public class HexMapAdminController : ControllerBase
    {
        private readonly IExpeditionService _expeditionService;
        private readonly IMapMergeService _mapMergeService;
        private readonly TerrainGenerator _terrainGenerator;
        private readonly ApplicationDbContext _context;

        public HexMapAdminController(
            IExpeditionService expeditionService,
            IMapMergeService mapMergeService,
            TerrainGenerator terrainGenerator,
            ApplicationDbContext context)
        {
            _expeditionService = expeditionService;
            _mapMergeService = mapMergeService;
            _terrainGenerator = terrainGenerator;
            _context = context;
        }

        // ===== EXPEDITION MANAGEMENT =====

        // POST: api/HexMapAdmin/expeditions/create
        [HttpPost("expeditions/create")]
        public async Task<IActionResult> CreateExpedition([FromBody] CreateExpeditionRequest request)
        {
            var expedition = await _expeditionService.CreateExpeditionAsync(
                request.Name,
                request.Description,
                request.LeaderUserId,
                request.StartQ,
                request.StartR
            );

            return Ok(expedition);
        }

        // POST: api/HexMapAdmin/expeditions/{id}/end
        [HttpPost("expeditions/{id}/end")]
        public async Task<IActionResult> EndExpedition(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _expeditionService.EndExpeditionAsync(id, userId.Value);

            if (!result)
            {
                return BadRequest(new { message = "Cannot end expedition. It may not exist or is already completed." });
            }

            // Trigger merge to Town Map
            var mergeResult = await _mapMergeService.MergeExpeditionToTownMapAsync(id);

            return Ok(new
            {
                message = "Expedition ended and merged to Town Map",
                mergeResult
            });
        }

        // ===== TOWN MAP MANAGEMENT =====

        // GET: api/HexMapAdmin/town-map/disputed
        [HttpGet("town-map/disputed")]
        public async Task<IActionResult> GetDisputedHexes()
        {
            var disputedHexes = await _mapMergeService.GetDisputedHexesAsync();
            return Ok(disputedHexes);
        }

        // POST: api/HexMapAdmin/town-map/resolve-dispute
        [HttpPost("town-map/resolve-dispute")]
        public async Task<IActionResult> ResolveDispute([FromBody] ResolveDisputeRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _mapMergeService.ResolveDisputeAsync(
                request.TownMapHexId,
                request.CorrectTerrain,
                userId.Value
            );

            if (!result)
            {
                return BadRequest(new { message = "Cannot resolve dispute. Hex may not exist or is not disputed." });
            }

            return Ok(new { message = "Dispute resolved successfully" });
        }

        // GET: api/HexMapAdmin/town-map/hexes
        [HttpGet("town-map/hexes")]
        public async Task<IActionResult> GetTownMapHexes()
        {
            var hexes = await _context.TownMapHexes
                .Include(h => h.DiscoveryHistory)
                .OrderBy(h => h.FirstDiscoveredDate)
                .ToListAsync();

            return Ok(hexes);
        }

        // ===== GM MAP MANAGEMENT =====

        // GET: api/HexMapAdmin/gm-map/hexes
        [HttpGet("gm-map/hexes")]
        public async Task<IActionResult> GetGMMapHexes()
        {
            var hexes = await _context.GMMapHexes
                .OrderBy(h => h.CreatedDate)
                .ToListAsync();

            return Ok(hexes);
        }

        // POST: api/HexMapAdmin/gm-map/set-terrain
        [HttpPost("gm-map/set-terrain")]
        public async Task<IActionResult> SetGMMapTerrain([FromBody] SetTerrainRequest request)
        {
            var hex = await _context.GMMapHexes
                .FirstOrDefaultAsync(h => h.Q == request.Q && h.R == request.R);

            if (hex == null)
            {
                // Create new hex
                hex = new GMMapHex
                {
                    Q = request.Q,
                    R = request.R,
                    Terrain = request.Terrain,
                    IsManuallySet = true,
                    GmNotes = request.GmNotes,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                _context.GMMapHexes.Add(hex);
            }
            else
            {
                // Update existing hex
                hex.Terrain = request.Terrain;
                hex.IsManuallySet = true;
                hex.GmNotes = request.GmNotes;
                hex.LastModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "GM Map terrain set successfully", hex });
        }

        // POST: api/HexMapAdmin/gm-map/generate-hex
        [HttpPost("gm-map/generate-hex")]
        public async Task<IActionResult> GenerateHexTerrain([FromBody] GenerateHexRequest request)
        {
            // Check if hex already exists
            var existingHex = await _context.GMMapHexes
                .FirstOrDefaultAsync(h => h.Q == request.Q && h.R == request.R);

            if (existingHex != null && existingHex.IsManuallySet)
            {
                return BadRequest(new { message = "Hex is manually set and locked from generation" });
            }

            // Get neighboring hexes for terrain generation
            var coordinate = new HexCoordinate(request.Q, request.R);
            var neighbors = HexMath.GetNeighbors(coordinate);

            var existingHexes = new Dictionary<HexCoordinate, TerrainType>();
            foreach (var neighbor in neighbors)
            {
                var neighborHex = await _context.GMMapHexes
                    .FirstOrDefaultAsync(h => h.Q == neighbor.Q && h.R == neighbor.R);

                if (neighborHex != null)
                {
                    existingHexes[neighbor] = neighborHex.Terrain;
                }
            }

            // Generate terrain
            var terrain = _terrainGenerator.GenerateTerrain(coordinate, existingHexes);

            if (existingHex == null)
            {
                // Create new hex
                var hex = new GMMapHex
                {
                    Q = request.Q,
                    R = request.R,
                    Terrain = terrain,
                    IsManuallySet = false,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                _context.GMMapHexes.Add(hex);
            }
            else
            {
                // Update existing hex
                existingHex.Terrain = terrain;
                existingHex.LastModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Hex terrain generated successfully", terrain });
        }

        // ===== POI MANAGEMENT =====

        // POST: api/HexMapAdmin/poi/create
        [HttpPost("poi/create")]
        public async Task<IActionResult> CreatePointOfInterest([FromBody] CreatePOIRequest request)
        {
            var poi = new PointOfInterest
            {
                Name = request.Name,
                Description = request.Description,
                TrueQ = request.TrueQ,
                TrueR = request.TrueR,
                PlayerKnownQ = request.PlayerKnownQ,
                PlayerKnownR = request.PlayerKnownR,
                IsLocationVerified = request.IsLocationVerified,
                LoreArticleUrl = request.LoreArticleUrl,
                CreatedDate = DateTime.UtcNow
            };

            // Link to GM Map hex if exists
            var gmHex = await _context.GMMapHexes
                .FirstOrDefaultAsync(h => h.Q == request.TrueQ && h.R == request.TrueR);

            if (gmHex != null)
            {
                poi.GMMapHexId = gmHex.Id;
            }

            // Link to Town Map hex if player location is known
            if (request.PlayerKnownQ.HasValue && request.PlayerKnownR.HasValue)
            {
                var townHex = await _context.TownMapHexes
                    .FirstOrDefaultAsync(h => h.Q == request.PlayerKnownQ && h.R == request.PlayerKnownR);

                if (townHex != null)
                {
                    poi.TownMapHexId = townHex.Id;
                }
            }

            _context.PointsOfInterest.Add(poi);
            await _context.SaveChangesAsync();

            return Ok(poi);
        }

        // GET: api/HexMapAdmin/poi/all
        [HttpGet("poi/all")]
        public async Task<IActionResult> GetAllPOIs()
        {
            var pois = await _context.PointsOfInterest
                .OrderBy(p => p.CreatedDate)
                .ToListAsync();

            return Ok(pois);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;
            return int.Parse(userIdClaim.Value);
        }
    }

    // DTOs
    public class CreateExpeditionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LeaderUserId { get; set; }
        public int StartQ { get; set; }
        public int StartR { get; set; }
    }

    public class ResolveDisputeRequest
    {
        public int TownMapHexId { get; set; }
        public TerrainType CorrectTerrain { get; set; }
    }

    public class SetTerrainRequest
    {
        public int Q { get; set; }
        public int R { get; set; }
        public TerrainType Terrain { get; set; }
        public string? GmNotes { get; set; }
    }

    public class GenerateHexRequest
    {
        public int Q { get; set; }
        public int R { get; set; }
    }

    public class CreatePOIRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TrueQ { get; set; }
        public int TrueR { get; set; }
        public int? PlayerKnownQ { get; set; }
        public int? PlayerKnownR { get; set; }
        public bool IsLocationVerified { get; set; }
        public string? LoreArticleUrl { get; set; }
    }
}