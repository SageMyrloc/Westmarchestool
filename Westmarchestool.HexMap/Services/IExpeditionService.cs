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
        Task<Expedition> CompleteExpeditionAsync(int expeditionId);

        // Member management
        Task AddMemberAsync(int expeditionId, int characterId, int playerId);
        Task RemoveMemberAsync(int expeditionId, int characterId);

        // Exploration tracking
        Task<ExpeditionHex> RecordExplorationAsync(int expeditionId, AxialHex believedPosition, AxialHex actualPosition, string terrainType);
        Task<List<ExpeditionHex>> GetExpeditionMapAsync(int expeditionId);

        // Getting lost mechanics
        Task<bool> IsExpeditionLostAsync(int expeditionId);
        Task CorrectExpeditionPositionAsync(int expeditionId, AxialHex actualPosition);

        // Synchronization (town return)
        Task SynchronizeToTownMapAsync(int expeditionId);
    }
}