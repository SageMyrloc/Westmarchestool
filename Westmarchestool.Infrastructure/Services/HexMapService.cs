using Microsoft.EntityFrameworkCore;
using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.HexMap.Entities;
using Westmarchestool.HexMap.Services;
using Westmarchestool.Infrastructure.Data;

namespace Westmarchestool.Infrastructure.Services
{
    public class HexMapService : IHexMapService
    {
        private readonly ApplicationDbContext _context;

        public HexMapService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========== Basic CRUD Operations ==========

        public async Task<HexTile?> GetHexAsync(int q, int r)
        {
            return await _context.HexTiles.FindAsync(q, r);
        }

        public async Task<HexTile?> GetHexAsync(AxialHex axial)
        {
            return await GetHexAsync(axial.Q, axial.R);
        }

        public async Task<List<HexTile>> GetHexesInRangeAsync(AxialHex center, int range)
        {
            // Get all cube hexes in range
            var cubeHexes = HexMath.GetHexesInRange(center, range);

            // Convert to axial and query database
            var hexes = new List<HexTile>();

            foreach (var cubeHex in cubeHexes)
            {
                var axial = HexConverter.CubeToAxial(cubeHex);
                var hex = await GetHexAsync(axial);
                if (hex != null)
                {
                    hexes.Add(hex);
                }
            }

            return hexes;
        }

        public async Task<HexTile> CreateHexAsync(AxialHex position, string terrainType)
        {
            // Check if hex already exists
            var existing = await GetHexAsync(position);
            if (existing != null)
            {
                throw new InvalidOperationException($"Hex at ({position.Q}, {position.R}) already exists");
            }

            var hex = new HexTile
            {
                Q = position.Q,
                R = position.R,
                TerrainType = terrainType,
                IsExploredByGM = true,
                IsOnTownMap = false, // Not public by default
                CreatedDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            _context.HexTiles.Add(hex);
            await _context.SaveChangesAsync();

            return hex;
        }

        public async Task<HexTile> UpdateHexAsync(int q, int r, string terrainType)
        {
            var hex = await GetHexAsync(q, r);
            if (hex == null)
            {
                throw new InvalidOperationException($"Hex at ({q}, {r}) not found");
            }

            hex.TerrainType = terrainType;
            hex.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return hex;
        }

        // ========== Map-Specific Operations ==========

        public async Task<List<HexTile>> GetGMMapAsync()
        {
            return await _context.HexTiles
                .Where(h => h.IsExploredByGM)
                .ToListAsync();
        }

        public async Task<List<HexTile>> GetTownMapAsync()
        {
            return await _context.HexTiles
                .Where(h => h.IsOnTownMap)
                .ToListAsync();
        }

        public async Task MarkHexAsPublicAsync(int q, int r)
        {
            var hex = await GetHexAsync(q, r);
            if (hex == null)
            {
                throw new InvalidOperationException($"Hex at ({q}, {r}) not found");
            }

            hex.IsOnTownMap = true;
            hex.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ========== Neighbor Operations ==========

        public async Task<List<HexTile>> GetNeighborsAsync(AxialHex hex)
        {
            var neighbors = hex.GetAllNeighbors();
            var hexNeighbors = new List<HexTile>();

            foreach (var neighbor in neighbors)
            {
                var neighborHex = await GetHexAsync(neighbor);
                if (neighborHex != null)
                {
                    hexNeighbors.Add(neighborHex);
                }
            }

            return hexNeighbors;
        }

        // ========== Generation Operations ==========

        public async Task GenerateBorderHexesAsync(AxialHex center, int borderDistance)
        {
            // Get all hexes in range (including center)
            var hexesInRange = HexMath.GetHexesInRange(
                HexConverter.AxialToCube(center),
                borderDistance
            );

            foreach (var cubeHex in hexesInRange)
            {
                var axial = HexConverter.CubeToAxial(cubeHex);

                // Check if hex already exists
                var existing = await GetHexAsync(axial);
                if (existing == null)
                {
                    // Generate new hex
                    var terrain = await GenerateTerrainForPositionAsync(axial);
                    await CreateHexAsync(axial, terrain);
                }
            }
        }

        // ========== Private Helper Methods ==========

        private async Task<string> GenerateTerrainForPositionAsync(AxialHex position)
        {
            // Get neighbors to influence terrain generation
            var neighbors = await GetNeighborsAsync(position);

            if (neighbors.Count == 0)
            {
                // No neighbors, use default terrain
                return "Grassland";
            }

            // Count terrain types in neighbors
            var terrainCounts = neighbors
                .GroupBy(n => n.TerrainType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Most common neighbor terrain has higher chance
            var mostCommon = terrainCounts.OrderByDescending(kvp => kvp.Value).First().Key;

            // 60% chance to match most common neighbor, 40% random
            var random = new Random();
            if (random.NextDouble() < 0.6)
            {
                return mostCommon;
            }

            // Random terrain
            var terrainTypes = new[] { "Grassland", "Forest", "Mountain", "Swamp", "Desert", "Hills" };
            return terrainTypes[random.Next(terrainTypes.Length)];
        }
    }
}