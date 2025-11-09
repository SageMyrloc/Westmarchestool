using Microsoft.EntityFrameworkCore;
using Westmarchestool.Core.Entities.HexMap;
using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.Infrastructure.Data;


namespace Westmarchestool.HexMap.Services
{
    public class ExpeditionService : IExpeditionService
    {
        private readonly ApplicationDbContext _context;

        public ExpeditionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== Expedition CRUD =====

        public async Task<Expedition> CreateExpeditionAsync(string name, string? description, int leaderUserId, int startQ, int startR)
        {
            var expedition = new Expedition
            {
                Name = name,
                Description = description,
                LeaderUserId = leaderUserId,
                StartQ = startQ,
                StartR = startR,
                Status = ExpeditionStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            _context.Expeditions.Add(expedition);
            await _context.SaveChangesAsync();

            // Automatically add leader as first member
            await JoinExpeditionAsync(expedition.Id, leaderUserId);

            // Initialize expedition map with Town Map knowledge
            await InitializeExpeditionMapAsync(expedition.Id);

            return expedition;
        }

        public async Task<Expedition?> GetExpeditionByIdAsync(int expeditionId)
        {
            return await _context.Expeditions
                .Include(e => e.Leader)
                .Include(e => e.Members)
                    .ThenInclude(m => m.User)
                .Include(e => e.Hexes)
                .FirstOrDefaultAsync(e => e.Id == expeditionId);
        }

        public async Task<List<Expedition>> GetActiveExpeditionsAsync()
        {
            return await _context.Expeditions
                .Include(e => e.Leader)
                .Include(e => e.Members)
                    .ThenInclude(m => m.User)
                .Where(e => e.Status == ExpeditionStatus.Active)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Expedition>> GetExpeditionsByUserAsync(int userId)
        {
            return await _context.Expeditions
                .Include(e => e.Leader)
                .Include(e => e.Members)
                    .ThenInclude(m => m.User)
                .Where(e => e.Members.Any(m => m.UserId == userId && m.LeftDate == null))
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> EndExpeditionAsync(int expeditionId, int requestingUserId)
        {
            var expedition = await GetExpeditionByIdAsync(expeditionId);
            if (expedition == null || expedition.Status != ExpeditionStatus.Active)
                return false;

            // Only GM or admin can end expeditions (check will be in controller)
            expedition.Status = ExpeditionStatus.Completed;
            expedition.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // ===== Membership Management =====

        public async Task<bool> JoinExpeditionAsync(int expeditionId, int userId)
        {
            var expedition = await _context.Expeditions.FindAsync(expeditionId);
            if (expedition == null || expedition.Status != ExpeditionStatus.Active)
                return false;

            // Check if user is already in an active expedition
            var isInExpedition = await IsUserInActiveExpeditionAsync(userId);
            if (isInExpedition)
                return false;

            // Check if user is already a member (and hasn't left)
            var existingMembership = await _context.ExpeditionMembers
                .FirstOrDefaultAsync(m => m.ExpeditionId == expeditionId && m.UserId == userId && m.LeftDate == null);

            if (existingMembership != null)
                return false; // Already a member

            var member = new ExpeditionMember
            {
                ExpeditionId = expeditionId,
                UserId = userId,
                JoinedDate = DateTime.UtcNow
            };

            _context.ExpeditionMembers.Add(member);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LeaveExpeditionAsync(int expeditionId, int userId, bool pushToTownMap)
        {
            var member = await _context.ExpeditionMembers
                .FirstOrDefaultAsync(m => m.ExpeditionId == expeditionId && m.UserId == userId && m.LeftDate == null);

            if (member == null)
                return false;

            member.LeftDate = DateTime.UtcNow;
            member.PushedToTownMap = pushToTownMap;
            member.PushDate = pushToTownMap ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsUserInActiveExpeditionAsync(int userId)
        {
            return await _context.ExpeditionMembers
                .AnyAsync(m => m.UserId == userId && m.LeftDate == null && m.Expedition.Status == ExpeditionStatus.Active);
        }

        public async Task<Expedition?> GetUserActiveExpeditionAsync(int userId)
        {
            var member = await _context.ExpeditionMembers
                .Include(m => m.Expedition)
                    .ThenInclude(e => e.Leader)
                .Include(m => m.Expedition)
                    .ThenInclude(e => e.Members)
                        .ThenInclude(em => em.User)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.LeftDate == null && m.Expedition.Status == ExpeditionStatus.Active);

            return member?.Expedition;
        }

        // ===== Leader Management =====

        public async Task<bool> ReassignLeaderAsync(int expeditionId, int newLeaderUserId, int requestingUserId)
        {
            var expedition = await GetExpeditionByIdAsync(expeditionId);
            if (expedition == null || expedition.Status != ExpeditionStatus.Active)
                return false;

            // Check if new leader is a current member
            var isNewLeaderMember = expedition.Members.Any(m => m.UserId == newLeaderUserId && m.LeftDate == null);
            if (!isNewLeaderMember)
                return false;

            expedition.LeaderUserId = newLeaderUserId;
            await _context.SaveChangesAsync();

            return true;
        }

        // ===== Hex Discovery =====

        public async Task<bool> MarkHexAsync(int expeditionId, int leaderUserId, HexCoordinate coordinate, TerrainType terrain, string? notes)
        {
            var expedition = await _context.Expeditions.FindAsync(expeditionId);
            if (expedition == null || expedition.Status != ExpeditionStatus.Active)
                return false;

            // Verify requesting user is the leader
            if (expedition.LeaderUserId != leaderUserId)
                return false;

            // Check if hex already marked
            var existingHex = await GetExpeditionHexAsync(expeditionId, coordinate);
            if (existingHex != null)
            {
                // Update existing hex
                existingHex.Terrain = terrain;
                existingHex.Notes = notes;
                existingHex.DiscoveredDate = DateTime.UtcNow;
            }
            else
            {
                // Add new hex
                var expeditionHex = new ExpeditionHex
                {
                    ExpeditionId = expeditionId,
                    Q = coordinate.Q,
                    R = coordinate.R,
                    Terrain = terrain,
                    Notes = notes,
                    DiscoveredDate = DateTime.UtcNow
                };

                _context.ExpeditionHexes.Add(expeditionHex);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ExpeditionHex>> GetExpeditionHexesAsync(int expeditionId)
        {
            return await _context.ExpeditionHexes
                .Where(h => h.ExpeditionId == expeditionId)
                .OrderBy(h => h.DiscoveredDate)
                .ToListAsync();
        }

        public async Task<ExpeditionHex?> GetExpeditionHexAsync(int expeditionId, HexCoordinate coordinate)
        {
            return await _context.ExpeditionHexes
                .FirstOrDefaultAsync(h => h.ExpeditionId == expeditionId && h.Q == coordinate.Q && h.R == coordinate.R);
        }

        // ===== Expedition Map Initialization =====

        public async Task InitializeExpeditionMapAsync(int expeditionId)
        {
            var expedition = await _context.Expeditions.FindAsync(expeditionId);
            if (expedition == null)
                return;

            // Copy all Town Map hexes to Expedition Map
            var townMapHexes = await _context.TownMapHexes
                .Where(h => h.Status != HexStatus.Unexplored)
                .ToListAsync();

            foreach (var townHex in townMapHexes)
            {
                var expeditionHex = new ExpeditionHex
                {
                    ExpeditionId = expeditionId,
                    Q = townHex.Q,
                    R = townHex.R,
                    Terrain = townHex.Terrain,
                    Notes = null, // No notes from town map
                    DiscoveredDate = DateTime.UtcNow
                };

                _context.ExpeditionHexes.Add(expeditionHex);
            }

            await _context.SaveChangesAsync();
        }
    }
}