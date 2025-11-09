using Westmarchestool.Core.Entities.HexMap;
using Westmarchestool.HexMap.Coordinates;

namespace Westmarchestool.HexMap.Services
{
    public interface IExpeditionService
    {
        // Expedition CRUD
        Task<Expedition> CreateExpeditionAsync(string name, string? description, int leaderUserId, int startQ, int startR);
        Task<Expedition?> GetExpeditionByIdAsync(int expeditionId);
        Task<List<Expedition>> GetActiveExpeditionsAsync();
        Task<List<Expedition>> GetExpeditionsByUserAsync(int userId);
        Task<bool> EndExpeditionAsync(int expeditionId, int requestingUserId);

        // Membership management
        Task<bool> JoinExpeditionAsync(int expeditionId, int userId);
        Task<bool> LeaveExpeditionAsync(int expeditionId, int userId, bool pushToTownMap);
        Task<bool> IsUserInActiveExpeditionAsync(int userId);
        Task<Expedition?> GetUserActiveExpeditionAsync(int userId);

        // Leader management
        Task<bool> ReassignLeaderAsync(int expeditionId, int newLeaderUserId, int requestingUserId);

        // Hex discovery (Leader only)
        Task<bool> MarkHexAsync(int expeditionId, int leaderUserId, HexCoordinate coordinate, TerrainType terrain, string? notes);
        Task<List<ExpeditionHex>> GetExpeditionHexesAsync(int expeditionId);
        Task<ExpeditionHex?> GetExpeditionHexAsync(int expeditionId, HexCoordinate coordinate);

        // Expedition map initialization (copy from Town Map)
        Task InitializeExpeditionMapAsync(int expeditionId);
    }
}