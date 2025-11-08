using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.HexMap.Entities;
using Westmarchestool.HexMap.Services;
using Westmarchestool.Infrastructure.Data;

namespace Westmarchestool.Infrastructure.Services
{
    public class ExpeditionService : IExpeditionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHexMapService _hexMapService;

        public ExpeditionService(ApplicationDbContext context, IHexMapService hexMapService)
        {
            _context = context;
            _hexMapService = hexMapService;
        }

        // ========== Expedition Lifecycle ==========

        public async Task<Expedition> CreateExpeditionAsync(string groupName, int leaderPlayerId, AxialHex startPosition)
        {
            // Verify leader exists
            var leader = await _context.Users.FindAsync(leaderPlayerId);
            if (leader == null)
            {
                throw new InvalidOperationException($"User with ID {leaderPlayerId} not found");
            }

            // Verify starting hex exists
            var startHex = await _hexMapService.GetHexAsync(startPosition);
            if (startHex == null)
            {
                throw new InvalidOperationException($"Starting hex at ({startPosition.Q}, {startPosition.R}) not found");
            }

            var expedition = new Expedition
            {
                GroupName = groupName,
                LeaderPlayerId = leaderPlayerId,
                DepartureTime = DateTime.UtcNow,
                StartQ = startPosition.Q,
                StartR = startPosition.R,
                LastKnownQ = startPosition.Q,
                LastKnownR = startPosition.R,
                Status = "Active"
            };

            _context.Expeditions.Add(expedition);
            await _context.SaveChangesAsync();

            return expedition;
        }

        public async Task<Expedition?> GetExpeditionAsync(int expeditionId)
        {
            return await _context.Expeditions
                .Include(e => e.LeaderPlayer)
                .Include(e => e.Members)
                    .ThenInclude(m => m.Character)
                .Include(e => e.Members)
                    .ThenInclude(m => m.Player)
                .Include(e => e.ExploredHexes)
                .FirstOrDefaultAsync(e => e.Id == expeditionId);
        }

        public async Task<List<Expedition>> GetActiveExpeditionsAsync()
        {
            return await _context.Expeditions
                .Include(e => e.LeaderPlayer)
                .Include(e => e.Members)
                .Where(e => e.Status == "Active")
                .OrderByDescending(e => e.DepartureTime)
                .ToListAsync();
        }

        public async Task<Expedition> CompleteExpeditionAsync(int expeditionId)
        {
            var expedition = await GetExpeditionAsync(expeditionId);
            if (expedition == null)
            {
                throw new InvalidOperationException($"Expedition {expeditionId} not found");
            }

            if (expedition.Status != "Active")
            {
                throw new InvalidOperationException($"Expedition {expeditionId} is not active");
            }

            // Mark expedition complete (NO auto-sync to town map!)
            expedition.Status = "Returned";
            expedition.ReturnTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return expedition;
        }

        // ========== Member Management ==========

        public async Task AddMemberAsync(int expeditionId, int characterId, int playerId)
        {
            // Verify expedition exists and is active
            var expedition = await _context.Expeditions.FindAsync(expeditionId);
            if (expedition == null)
            {
                throw new InvalidOperationException($"Expedition {expeditionId} not found");
            }

            if (expedition.Status != "Active")
            {
                throw new InvalidOperationException("Cannot add members to inactive expedition");
            }

            // Verify character exists
            var character = await _context.Characters.FindAsync(characterId);
            if (character == null)
            {
                throw new InvalidOperationException($"Character {characterId} not found");
            }

            // Check if already member
            var existingMember = await _context.ExpeditionMembers
                .FirstOrDefaultAsync(em => em.ExpeditionId == expeditionId && em.CharacterId == characterId);

            if (existingMember != null)
            {
                throw new InvalidOperationException("Character already in expedition");
            }

            var member = new ExpeditionMember
            {
                ExpeditionId = expeditionId,
                CharacterId = characterId,
                PlayerId = playerId,
                JoinedAt = DateTime.UtcNow
            };

            _context.ExpeditionMembers.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int expeditionId, int characterId)
        {
            var member = await _context.ExpeditionMembers
                .FirstOrDefaultAsync(em => em.ExpeditionId == expeditionId && em.CharacterId == characterId);

            if (member == null)
            {
                throw new InvalidOperationException("Member not found in expedition");
            }

            member.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ========== Exploration Tracking ==========
        public async Task<ExpeditionHex> RecordExplorationAsync(
            int expeditionId,
            AxialHex believedPosition,
            AxialHex actualPosition,
            string terrainType)
        {
            var expedition = await _context.Expeditions.FindAsync(expeditionId);
            if (expedition == null)
            {
                throw new InvalidOperationException($"Expedition {expeditionId} not found");
            }

            var exploredHex = new ExpeditionHex
            {
                ExpeditionId = expeditionId,
                Q = believedPosition.Q,
                R = believedPosition.R,
                ActualQ = actualPosition.Q,
                ActualR = actualPosition.R,
                TerrainType = terrainType,
                IsAccurate = (believedPosition.Q == actualPosition.Q && believedPosition.R == actualPosition.R),
                ExploredTime = DateTime.UtcNow
            };

            _context.ExpeditionHexes.Add(exploredHex);

            // Update expedition's last known position (actual, not believed)
            expedition.LastKnownQ = actualPosition.Q;
            expedition.LastKnownR = actualPosition.R;

            await _context.SaveChangesAsync();

            return exploredHex;
        }

        public async Task<List<ExpeditionHex>> GetExpeditionMapAsync(int expeditionId)
        {
            return await _context.ExpeditionHexes
                .Where(eh => eh.ExpeditionId == expeditionId)
                .OrderBy(eh => eh.ExploredTime)
                .ToListAsync();
        }

        // ========== Getting Lost Mechanics ==========

        public async Task<bool> IsExpeditionLostAsync(int expeditionId)
        {
            var exploredHexes = await GetExpeditionMapAsync(expeditionId);

            // Check if any recent explorations were inaccurate
            var recentHexes = exploredHexes
                .OrderByDescending(eh => eh.ExploredTime)
                .Take(5); // Check last 5 hexes

            return recentHexes.Any(eh => !eh.IsAccurate);
        }

        public async Task CorrectExpeditionPositionAsync(int expeditionId, AxialHex actualPosition)
        {
            var expedition = await _context.Expeditions.FindAsync(expeditionId);
            if (expedition == null)
            {
                throw new InvalidOperationException($"Expedition {expeditionId} not found");
            }

            // Update to correct position
            expedition.LastKnownQ = actualPosition.Q;
            expedition.LastKnownR = actualPosition.R;

            await _context.SaveChangesAsync();
        }

        // ========== PLAYER-DRIVEN SUBMISSION ==========

        public async Task<TownMapSubmission> SubmitExpeditionToTownAsync(int expeditionId, int submittingPlayerId)
        {
            var expedition = await GetExpeditionAsync(expeditionId);
            if (expedition == null)
            {
                throw new InvalidOperationException($"Expedition {expeditionId} not found");
            }

            if (expedition.Status != "Returned")
            {
                throw new InvalidOperationException("Can only submit completed expeditions");
            }

            // Check if already submitted
            var existingSubmission = await _context.TownMapSubmissions
                .FirstOrDefaultAsync(s => s.ExpeditionId == expeditionId);

            if (existingSubmission != null)
            {
                throw new InvalidOperationException("Expedition already submitted");
            }

            // Create submission record
            var submission = new TownMapSubmission
            {
                ExpeditionId = expeditionId,
                SubmittedByPlayerId = submittingPlayerId,
                Status = "Pending"
            };

            _context.TownMapSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Process each explored hex
            var exploredHexes = expedition.ExploredHexes.ToList();
            int accepted = 0;
            int conflicted = 0;

            foreach (var exploredHex in exploredHexes)
            {
                // Use ACTUAL position (not believed)
                var actualPosition = new AxialHex(exploredHex.ActualQ, exploredHex.ActualR);

                // Check if hex exists in Town Map
                var existingHex = await _hexMapService.GetHexAsync(actualPosition);

                if (existingHex == null)
                {
                    // NEW HEX - Add to Town Map (auto-accept)
                    var newHex = await _hexMapService.CreateHexAsync(actualPosition, exploredHex.TerrainType);
                    newHex.IsOnTownMap = true;
                    newHex.DiscoveredByExpeditionId = expeditionId;
                    newHex.DiscoveredDate = exploredHex.ExploredTime;

                    await _context.SaveChangesAsync();
                    accepted++;
                }
                else if (existingHex.IsOnTownMap)
                {
                    // HEX EXISTS - Check for conflict
                    if (existingHex.TerrainType != exploredHex.TerrainType)
                    {
                        // CONFLICT! Create conflict record
                        var conflict = new MapConflict
                        {
                            Q = actualPosition.Q,
                            R = actualPosition.R,
                            SubmissionId = submission.Id,
                            NewSubmitterId = submittingPlayerId,
                            ExistingSubmitterId = existingHex.DiscoveredByExpeditionId.HasValue
                                ? (await _context.Expeditions.FindAsync(existingHex.DiscoveredByExpeditionId.Value))?.LeaderPlayerId
                                : null,
                            NewTerrainType = exploredHex.TerrainType,
                            ExistingTerrainType = existingHex.TerrainType,
                            Status = "Unresolved"
                        };

                        _context.MapConflicts.Add(conflict);
                        conflicted++;
                    }
                    else
                    {
                        // Same terrain - confirmation, no conflict
                        accepted++;
                    }
                }
                else
                {
                    // Hex exists in GM map but not public yet - add to town map
                    existingHex.IsOnTownMap = true;
                    existingHex.DiscoveredByExpeditionId = expeditionId;
                    existingHex.DiscoveredDate = exploredHex.ExploredTime;
                    accepted++;
                }
            }

            // Update submission stats
            submission.HexesSubmitted = exploredHexes.Count;
            submission.HexesAccepted = accepted;
            submission.HexesConflicted = conflicted;
            submission.Status = conflicted > 0 ? "HasConflicts" : "Approved";

            await _context.SaveChangesAsync();

            return submission;
        }

        public async Task<TownMapSubmission?> GetSubmissionAsync(int submissionId)
        {
            return await _context.TownMapSubmissions
                .Include(s => s.Expedition)
                .Include(s => s.SubmittedByPlayer)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<List<TownMapSubmission>> GetPendingSubmissionsAsync()
        {
            return await _context.TownMapSubmissions
                .Include(s => s.Expedition)
                .Include(s => s.SubmittedByPlayer)
                .Where(s => s.Status == "Pending" || s.Status == "HasConflicts")
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }

        // ========== CONFLICT MANAGEMENT ==========

        public async Task<List<MapConflict>> GetUnresolvedConflictsAsync()
        {
            return await _context.MapConflicts
                .Include(c => c.NewSubmitter)
                .Include(c => c.ExistingSubmitter)
                .Include(c => c.Votes)
                .Where(c => c.Status == "Unresolved" || c.Status == "Voting")
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<MapConflict?> GetConflictAsync(int conflictId)
        {
            return await _context.MapConflicts
                .Include(c => c.NewSubmitter)
                .Include(c => c.ExistingSubmitter)
                .Include(c => c.Votes)
                    .ThenInclude(v => v.Player)
                .FirstOrDefaultAsync(c => c.Id == conflictId);
        }

        public async Task ResolveConflictAsync(int conflictId, string resolution, string? notes)
        {
            var conflict = await GetConflictAsync(conflictId);
            if (conflict == null)
            {
                throw new InvalidOperationException($"Conflict {conflictId} not found");
            }

            var position = new AxialHex(conflict.Q, conflict.R);
            var hex = await _hexMapService.GetHexAsync(position);

            if (hex == null)
            {
                throw new InvalidOperationException($"Hex at ({conflict.Q}, {conflict.R}) not found");
            }

            // Apply resolution
            if (resolution == "AcceptNew")
            {
                hex.TerrainType = conflict.NewTerrainType;
            }
            // If "KeepExisting", do nothing to hex

            conflict.Status = "Resolved";
            conflict.Resolution = resolution;
            conflict.ResolutionNotes = notes;
            conflict.ResolvedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ========== VOTING ==========

        public async Task VoteOnConflictAsync(int conflictId, int playerId, bool voteForNew, string? comment)
        {
            var conflict = await _context.MapConflicts.FindAsync(conflictId);
            if (conflict == null)
            {
                throw new InvalidOperationException($"Conflict {conflictId} not found");
            }

            // Check if player already voted
            var existingVote = await _context.ConflictVotes
                .FirstOrDefaultAsync(v => v.ConflictId == conflictId && v.PlayerId == playerId);

            if (existingVote != null)
            {
                throw new InvalidOperationException("Player has already voted on this conflict");
            }

            var vote = new ConflictVote
            {
                ConflictId = conflictId,
                PlayerId = playerId,
                VoteForNew = voteForNew,
                Comment = comment
            };

            _context.ConflictVotes.Add(vote);

            // Update conflict status to Voting if not already
            if (conflict.Status == "Unresolved")
            {
                conflict.Status = "Voting";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<bool, int>> GetConflictVoteTallyAsync(int conflictId)
        {
            var votes = await _context.ConflictVotes
                .Where(v => v.ConflictId == conflictId)
                .ToListAsync();

            return new Dictionary<bool, int>
            {
                { true, votes.Count(v => v.VoteForNew) },   // Votes for new terrain
                { false, votes.Count(v => !v.VoteForNew) }  // Votes for existing terrain
            };
        }

    }
}