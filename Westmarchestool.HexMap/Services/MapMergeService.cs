using Microsoft.EntityFrameworkCore;
using Westmarchestool.Core.Entities.HexMap;
using Westmarchestool.Infrastructure.Data;


namespace Westmarchestool.HexMap.Services
{
    public class MapMergeService : IMapMergeService
    {
        private readonly ApplicationDbContext _context;

        public MapMergeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MapMergeResult> MergeExpeditionToTownMapAsync(int expeditionId)
        {
            var result = new MapMergeResult { Success = true };

            var expedition = await _context.Expeditions
                .Include(e => e.Hexes)
                .FirstOrDefaultAsync(e => e.Id == expeditionId);

            if (expedition == null)
            {
                result.Success = false;
                result.ErrorMessage = "Expedition not found";
                return result;
            }

            if (expedition.Status != ExpeditionStatus.Completed)
            {
                result.Success = false;
                result.ErrorMessage = "Expedition must be completed before merging";
                return result;
            }

            // Process each hex discovered by the expedition
            foreach (var expeditionHex in expedition.Hexes)
            {
                await ProcessHexMerge(expeditionHex, expeditionId, result);
            }

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<MapMergeResult> MergePlayerMapToTownMapAsync(int expeditionId, int userId)
        {
            var result = new MapMergeResult { Success = true };

            // Verify user was a member and has left
            var member = await _context.ExpeditionMembers
                .FirstOrDefaultAsync(m => m.ExpeditionId == expeditionId && m.UserId == userId && m.LeftDate != null);

            if (member == null)
            {
                result.Success = false;
                result.ErrorMessage = "User is not a former member of this expedition";
                return result;
            }

            if (member.PushedToTownMap)
            {
                result.Success = false;
                result.ErrorMessage = "User has already pushed their map";
                return result;
            }

            // Get expedition hexes as they were when user left (snapshot at their LeftDate)
            var expeditionHexes = await _context.ExpeditionHexes
                .Where(h => h.ExpeditionId == expeditionId && h.DiscoveredDate <= member.LeftDate)
                .ToListAsync();

            // Process each hex
            foreach (var expeditionHex in expeditionHexes)
            {
                await ProcessHexMerge(expeditionHex, expeditionId, result);
            }

            // Mark as pushed
            member.PushedToTownMap = true;
            member.PushDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return result;
        }

        private async Task ProcessHexMerge(ExpeditionHex expeditionHex, int expeditionId, MapMergeResult result)
        {
            // Check if hex exists on Town Map
            var townMapHex = await _context.TownMapHexes
                .Include(h => h.DiscoveryHistory)
                .FirstOrDefaultAsync(h => h.Q == expeditionHex.Q && h.R == expeditionHex.R);

            if (townMapHex == null)
            {
                // New hex - add to Town Map
                townMapHex = new TownMapHex
                {
                    Q = expeditionHex.Q,
                    R = expeditionHex.R,
                    Terrain = expeditionHex.Terrain,
                    Status = HexStatus.Verified,
                    FirstDiscoveredDate = DateTime.UtcNow,
                    LastVerifiedDate = DateTime.UtcNow
                };

                _context.TownMapHexes.Add(townMapHex);
                await _context.SaveChangesAsync(); // Save to get ID for history

                // Add discovery history
                var history = new HexDiscoveryHistory
                {
                    TownMapHexId = townMapHex.Id,
                    ExpeditionId = expeditionId,
                    ReportedTerrain = expeditionHex.Terrain,
                    DiscoveryDate = DateTime.UtcNow,
                    IsVerification = false
                };

                _context.HexDiscoveryHistory.Add(history);

                result.HexesAdded++;
            }
            else if (townMapHex.Terrain == expeditionHex.Terrain)
            {
                // Same terrain - confirms existing knowledge
                townMapHex.LastVerifiedDate = DateTime.UtcNow;

                // Add discovery history
                var history = new HexDiscoveryHistory
                {
                    TownMapHexId = townMapHex.Id,
                    ExpeditionId = expeditionId,
                    ReportedTerrain = expeditionHex.Terrain,
                    DiscoveryDate = DateTime.UtcNow,
                    IsVerification = false
                };

                _context.HexDiscoveryHistory.Add(history);

                result.HexesConfirmed++;
            }
            else
            {
                // Different terrain - DISPUTE!
                townMapHex.Status = HexStatus.Disputed;

                // Add discovery history
                var history = new HexDiscoveryHistory
                {
                    TownMapHexId = townMapHex.Id,
                    ExpeditionId = expeditionId,
                    ReportedTerrain = expeditionHex.Terrain,
                    DiscoveryDate = DateTime.UtcNow,
                    IsVerification = false
                };

                _context.HexDiscoveryHistory.Add(history);

                result.DisputesCreated++;
                result.NewDisputes.Add(townMapHex);
            }
        }

        public async Task<List<TownMapHex>> GetDisputedHexesAsync()
        {
            return await _context.TownMapHexes
                .Include(h => h.DiscoveryHistory)
                    .ThenInclude(dh => dh.Expedition)
                        .ThenInclude(e => e.Leader)
                .Where(h => h.Status == HexStatus.Disputed)
                .OrderBy(h => h.FirstDiscoveredDate)
                .ToListAsync();
        }

        public async Task<bool> ResolveDisputeAsync(int townMapHexId, TerrainType correctTerrain, int resolvedByUserId)
        {
            var hex = await _context.TownMapHexes.FindAsync(townMapHexId);
            if (hex == null || hex.Status != HexStatus.Disputed)
                return false;

            hex.Terrain = correctTerrain;
            hex.Status = HexStatus.Verified;
            hex.LastVerifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}