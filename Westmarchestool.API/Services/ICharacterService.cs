using Westmarchestool.API.DTOs;
using Westmarchestool.API.Models;

namespace Westmarchestool.API.Services
{
    public interface ICharacterService
    {
        Task<CharacterDto?> ImportCharacterAsync(int userId, ImportCharacterDto importDto);
        Task<bool> DeleteCharacterAsync(int characterId, int requestingUserId);
        Task<List<CharacterDto>> GetUserCharactersAsync(int userId);
        Task<CharacterDto?> GetCharacterByIdAsync(int characterId, int requestingUserId);
        Task<string?> GetCharacterJsonAsync(int characterId, int requestingUserId);
        Task<bool> ApproveCharacterAsync(int characterId, int adminUserId);
        Task<bool> UpdateCharacterStatusAsync(int characterId, CharacterStatus newStatus, int adminUserId);
        Task<List<CharacterDto>> GetPendingCharactersAsync();
    }
}