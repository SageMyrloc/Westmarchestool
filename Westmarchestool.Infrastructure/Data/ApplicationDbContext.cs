using Microsoft.EntityFrameworkCore;
using Westmarchestool.Core.Entities;
using Westmarchestool.HexMap.Entities;

namespace Westmarchestool.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<CharacterJsonData> CharacterJsonData { get; set; }
        public DbSet<CharacterInventoryItem> CharacterInventory { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionAttendee> SessionAttendees { get; set; }
        public DbSet<CharacterHistoryEntry> CharacterHistory { get; set; }
        public DbSet<HexTile> HexTiles { get; set; }
        public DbSet<Expedition> Expeditions { get; set; }
        public DbSet<ExpeditionHex> ExpeditionHexes { get; set; }
        public DbSet<ExpeditionMember> ExpeditionMembers { get; set; }
        public DbSet<TownMapSubmission> TownMapSubmissions { get; set; }
        public DbSet<MapConflict> MapConflicts { get; set; }
        public DbSet<ConflictVote> ConflictVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure many-to-many relationship
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Configure Character relationships
            modelBuilder.Entity<Character>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Character>()
                .HasOne(c => c.JsonData)
                .WithOne(j => j.Character)
                .HasForeignKey<CharacterJsonData>(j => j.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Session relationships
            modelBuilder.Entity<Session>()
                .HasOne(s => s.GameMaster)
                .WithMany()
                .HasForeignKey(s => s.GameMasterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SessionAttendee relationships
            modelBuilder.Entity<SessionAttendee>()
                .HasOne(sa => sa.Session)
                .WithMany(s => s.Attendees)
                .HasForeignKey(sa => sa.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionAttendee>()
                .HasOne(sa => sa.Character)
                .WithMany(c => c.SessionAttendances)
                .HasForeignKey(sa => sa.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure HexTile composite primary key
            modelBuilder.Entity<HexTile>()
                .HasKey(h => new { h.Q, h.R });

            // Configure HexTile indexes
            modelBuilder.Entity<HexTile>()
                .HasIndex(h => h.TerrainType);

            modelBuilder.Entity<HexTile>()
                .HasIndex(h => h.IsExploredByGM);

            modelBuilder.Entity<HexTile>()
                .HasIndex(h => h.IsOnTownMap);

            // Configure Expedition relationships
            modelBuilder.Entity<Expedition>()
                .HasOne(e => e.LeaderPlayer)
                .WithMany()
                .HasForeignKey(e => e.LeaderPlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expedition>()
                .HasIndex(e => e.Status);

            // Configure ExpeditionHex relationships
            modelBuilder.Entity<ExpeditionHex>()
                .HasOne(eh => eh.Expedition)
                .WithMany(e => e.ExploredHexes)
                .HasForeignKey(eh => eh.ExpeditionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExpeditionHex>()
                .HasIndex(eh => new { eh.ExpeditionId, eh.Q, eh.R });

            // Configure ExpeditionMember relationships
            modelBuilder.Entity<ExpeditionMember>()
                .HasOne(em => em.Expedition)
                .WithMany(e => e.Members)
                .HasForeignKey(em => em.ExpeditionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExpeditionMember>()
                .HasOne(em => em.Character)
                .WithMany()
                .HasForeignKey(em => em.CharacterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExpeditionMember>()
                .HasOne(em => em.Player)
                .WithMany()
                .HasForeignKey(em => em.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TownMapSubmission relationships
            modelBuilder.Entity<TownMapSubmission>()
                .HasOne(s => s.Expedition)
                .WithMany()
                .HasForeignKey(s => s.ExpeditionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TownMapSubmission>()
                .HasOne(s => s.SubmittedByPlayer)
                .WithMany()
                .HasForeignKey(s => s.SubmittedByPlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TownMapSubmission>()
                .HasIndex(s => s.Status);

            // Configure MapConflict relationships
            modelBuilder.Entity<MapConflict>()
                .HasOne(c => c.Submission)
                .WithMany()
                .HasForeignKey(c => c.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MapConflict>()
                .HasOne(c => c.NewSubmitter)
                .WithMany()
                .HasForeignKey(c => c.NewSubmitterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapConflict>()
                .HasOne(c => c.ExistingSubmitter)
                .WithMany()
                .HasForeignKey(c => c.ExistingSubmitterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MapConflict>()
                .HasIndex(c => new { c.Q, c.R });

            modelBuilder.Entity<MapConflict>()
                .HasIndex(c => c.Status);

            // Configure ConflictVote relationships
            modelBuilder.Entity<ConflictVote>()
                .HasOne(v => v.Conflict)
                .WithMany(c => c.Votes)
                .HasForeignKey(v => v.ConflictId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConflictVote>()
                .HasOne(v => v.Player)
                .WithMany()
                .HasForeignKey(v => v.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate votes
            modelBuilder.Entity<ConflictVote>()
                .HasIndex(v => new { v.ConflictId, v.PlayerId })
                .IsUnique();

            // Seed initial roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Full system access" },
                new Role { Id = 2, Name = "GM", Description = "Game Master access" },
                new Role { Id = 3, Name = "Player", Description = "Player access" }
            );

            // Username must be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}