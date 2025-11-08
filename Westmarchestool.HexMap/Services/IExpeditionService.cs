using Westmarchestool.HexMap.Coordinates;
using Westmarchestool.HexMap.Entities;

namespace Westmarchestool.HexMap.Services
{
    /// <summary>
    /// Service interface for expedition operations
    /// </summary>
    public interface IExpeditionService
    {
        // Expedition lifecycle
        Task<Expedition> CreateExpeditionAsync(string groupName, int leaderPlayerId, AxialHex startPosition);
        Task<Expedition?> GetExpeditionAsync(int expeditionId);
        Task<List<Expedition>> GetActiveExpeditionsAsync();
        Task<Expedition> CompleteExpeditionAsync(int expeditionId); // Just marks complete, doesn't sync

        // Member management
        Task AddMemberAsync(int expeditionId, int characterId, int playerId);
        Task RemoveMemberAsync(int expeditionId, int characterId);

        // Exploration tracking
        Task<ExpeditionHex> RecordExplorationAsync(int expeditionId, AxialHex believedPosition, AxialHex actualPosition, string terrainType);
        Task<List<ExpeditionHex>> GetExpeditionMapAsync(int expeditionId);

        // Getting lost mechanics
        Task<bool> IsExpeditionLostAsync(int expeditionId);
        Task CorrectExpeditionPositionAsync(int expeditionId, AxialHex actualPosition);

        // PLAYER-DRIVEN SUBMISSION (NEW!)
        Task<TownMapSubmission> SubmitExpeditionToTownAsync(int expeditionId, int submittingPlayerId);
        Task<TownMapSubmission?> GetSubmissionAsync(int submissionId);
        Task<List<TownMapSubmission>> GetPendingSubmissionsAsync();

        // Conflict management
        Task<List<MapConflict>> GetUnresolvedConflictsAsync();
        Task<MapConflict?> GetConflictAsync(int conflictId);
        Task ResolveConflictAsync(int conflictId, string resolution, string? notes);

        // Voting
        Task VoteOnConflictAsync(int conflictId, int playerId, bool voteForNew, string? comment);
        Task<Dictionary<bool, int>> GetConflictVoteTallyAsync(int conflictId);
    }
}