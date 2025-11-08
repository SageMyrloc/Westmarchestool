using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Westmarchestool.Infrastructure.Data;
using Westmarchestool.API.DTOs;
using Westmarchestool.Core.Entities;

namespace Westmarchestool.API.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly ApplicationDbContext _context;

        public CharacterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CharacterDto?> ImportCharacterAsync(int userId, ImportCharacterDto importDto)
        {
            try
            {
                var root = importDto.PathbuilderJson;

                // Handle both formats: raw export has "success" wrapper, direct build doesn't
                JsonElement build;
                if (root.TryGetProperty("success", out _) && root.TryGetProperty("build", out var buildProp))
                {
                    // Format: {"success": true, "build": {...}}
                    build = buildProp;
                }
                else
                {
                    // Format: direct build object {...}
                    build = root;
                }

                // Extract key fields
                var character = new Character
                {
                    UserId = userId,
                    Name = build.GetProperty("name").GetString() ?? "Unknown",
                    Class = build.GetProperty("class").GetString() ?? "",
                    Level = build.GetProperty("level").GetInt32(),
                    Ancestry = build.GetProperty("ancestry").GetString() ?? "",
                    Background = build.GetProperty("background").GetString() ?? "",
                    Experience = build.TryGetProperty("xp", out var xp) ? xp.GetInt32() : 0,
                    PortraitUrl = importDto.PortraitUrl,
                    Status = CharacterStatus.Pending
                };

                // Extract money
                if (build.TryGetProperty("money", out var money))
                {
                    character.CopperPieces = money.TryGetProperty("cp", out var cp) ? cp.GetInt32() : 0;
                    character.SilverPieces = money.TryGetProperty("sp", out var sp) ? sp.GetInt32() : 0;
                    character.GoldPieces = money.TryGetProperty("gp", out var gp) ? gp.GetInt32() : 0;
                    character.PlatinumPieces = money.TryGetProperty("pp", out var pp) ? pp.GetInt32() : 0;
                }

                _context.Characters.Add(character);
                await _context.SaveChangesAsync();

                // Store the full JSON - serialize the JsonElement back to string
                var jsonData = new CharacterJsonData
                {
                    CharacterId = character.Id,
                    JsonContent = JsonSerializer.Serialize(importDto.PathbuilderJson)
                };
                _context.CharacterJsonData.Add(jsonData);

                // Add history entry
                var historyEntry = new CharacterHistoryEntry
                {
                    CharacterId = character.Id,
                    ChangeType = HistoryChangeType.Imported,
                    Description = $"Character imported from Pathbuilder",
                    ChangedByUserId = userId
                };
                _context.CharacterHistory.Add(historyEntry);

                await _context.SaveChangesAsync();

                return await GetCharacterByIdAsync(character.Id, userId);
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Parse error: {jsonEx.Message}");
                Console.WriteLine($"Path: {jsonEx.Path}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<CharacterDto>> GetUserCharactersAsync(int userId)
        {
            var characters = await _context.Characters
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return characters.Select(c => MapToDto(c)).ToList();
        }

        public async Task<CharacterDto?> GetCharacterByIdAsync(int characterId, int requestingUserId)
        {
            var character = await _context.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null) return null;

            // Check permissions: Owner, or GM/Admin
            var requestingUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == requestingUserId);

            if (requestingUser == null) return null;

            var isOwner = character.UserId == requestingUserId;
            var isGmOrAdmin = requestingUser.UserRoles.Any(ur =>
                ur.Role.Name == "GM" || ur.Role.Name == "Admin");

            if (!isOwner && !isGmOrAdmin) return null;

            return MapToDto(character);
        }

        public async Task<string?> GetCharacterJsonAsync(int characterId, int requestingUserId)
        {
            var character = await _context.Characters
                .Include(c => c.JsonData)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null) return null;

            // Check permissions
            var requestingUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == requestingUserId);

            if (requestingUser == null) return null;

            var isOwner = character.UserId == requestingUserId;
            var isGmOrAdmin = requestingUser.UserRoles.Any(ur =>
                ur.Role.Name == "GM" || ur.Role.Name == "Admin");

            if (!isOwner && !isGmOrAdmin) return null;

            return character.JsonData?.JsonContent;
        }

        public async Task<bool> ApproveCharacterAsync(int characterId, int adminUserId)
        {
            var character = await _context.Characters.FindAsync(characterId);
            if (character == null || character.Status != CharacterStatus.Pending)
                return false;

            character.Status = CharacterStatus.Active;
            character.ApprovedDate = DateTime.UtcNow;
            character.ApprovedByUserId = adminUserId;
            character.LastModifiedDate = DateTime.UtcNow;

            var historyEntry = new CharacterHistoryEntry
            {
                CharacterId = characterId,
                ChangeType = HistoryChangeType.Approved,
                Description = "Character approved by admin",
                ChangedByUserId = adminUserId
            };
            _context.CharacterHistory.Add(historyEntry);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCharacterStatusAsync(int characterId, CharacterStatus newStatus, int adminUserId)
        {
            var character = await _context.Characters.FindAsync(characterId);
            if (character == null) return false;

            var oldStatus = character.Status;
            character.Status = newStatus;
            character.LastModifiedDate = DateTime.UtcNow;

            var historyEntry = new CharacterHistoryEntry
            {
                CharacterId = characterId,
                ChangeType = HistoryChangeType.StatusChanged,
                Description = $"Status changed from {oldStatus} to {newStatus}",
                ChangedByUserId = adminUserId,
                OldValue = oldStatus.ToString(),
                NewValue = newStatus.ToString()
            };
            _context.CharacterHistory.Add(historyEntry);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CharacterDto>> GetPendingCharactersAsync()
        {
            var characters = await _context.Characters
                .Include(c => c.User)
                .Where(c => c.Status == CharacterStatus.Pending)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            return characters.Select(c => MapToDto(c)).ToList();
        }

        private CharacterDto MapToDto(Character character)
        {
            return new CharacterDto
            {
                Id = character.Id,
                Name = character.Name,
                Class = character.Class,
                Level = character.Level,
                Ancestry = character.Ancestry,
                Background = character.Background,
                Experience = character.Experience,
                Money = new MoneyDto
                {
                    Cp = character.CopperPieces,
                    Sp = character.SilverPieces,
                    Gp = character.GoldPieces,
                    Pp = character.PlatinumPieces
                },
                PortraitUrl = character.PortraitUrl,
                Status = character.Status,
                CreatedDate = character.CreatedDate,
                OwnerUsername = character.User?.Username ?? ""
            };
        }
    
    public async Task<bool> DeleteCharacterAsync(int characterId, int requestingUserId)
        {
            var character = await _context.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null) return false;

            // Check permissions: Owner can delete their own, Admin can delete any
            var requestingUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == requestingUserId);

            if (requestingUser == null) return false;

            var isOwner = character.UserId == requestingUserId;
            var isAdmin = requestingUser.UserRoles.Any(ur => ur.Role.Name == "Admin");

            if (!isOwner && !isAdmin) return false;

            // Delete character (cascade will handle related data)
            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}