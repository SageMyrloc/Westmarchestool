using Microsoft.EntityFrameworkCore;
using Westmarchestool.Core.Entities;
using Westmarchestool.Core.Entities.HexMap;

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
        // HexMap DbSets
        public DbSet<GMMapHex> GMMapHexes { get; set; }
        public DbSet<TownMapHex> TownMapHexes { get; set; }
        public DbSet<Expedition> Expeditions { get; set; }
        public DbSet<ExpeditionMember> ExpeditionMembers { get; set; }
        public DbSet<ExpeditionHex> ExpeditionHexes { get; set; }
        public DbSet<HexDiscoveryHistory> HexDiscoveryHistory { get; set; }
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }

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

            // ===== HexMap Configurations =====

            // GMMapHex - Unique coordinate constraint
            modelBuilder.Entity<GMMapHex>()
                .HasIndex(h => new { h.Q, h.R })
                .IsUnique();

            // TownMapHex - Unique coordinate constraint
            modelBuilder.Entity<TownMapHex>()
                .HasIndex(h => new { h.Q, h.R })
                .IsUnique();

            // Expedition - Leader relationship
            modelBuilder.Entity<Expedition>()
                .HasOne(e => e.Leader)
                .WithMany()
                .HasForeignKey(e => e.LeaderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExpeditionMember - Relationships
            modelBuilder.Entity<ExpeditionMember>()
                .HasOne(em => em.Expedition)
                .WithMany(e => e.Members)
                .HasForeignKey(em => em.ExpeditionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExpeditionMember>()
                .HasOne(em => em.User)
                .WithMany()
                .HasForeignKey(em => em.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExpeditionHex - Unique coordinate per expedition
            modelBuilder.Entity<ExpeditionHex>()
                .HasIndex(eh => new { eh.ExpeditionId, eh.Q, eh.R })
                .IsUnique();

            modelBuilder.Entity<ExpeditionHex>()
                .HasOne(eh => eh.Expedition)
                .WithMany(e => e.Hexes)
                .HasForeignKey(eh => eh.ExpeditionId)
                .OnDelete(DeleteBehavior.Cascade);

            // HexDiscoveryHistory - Relationships
            modelBuilder.Entity<HexDiscoveryHistory>()
                .HasOne(h => h.TownMapHex)
                .WithMany(t => t.DiscoveryHistory)
                .HasForeignKey(h => h.TownMapHexId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HexDiscoveryHistory>()
                .HasOne(h => h.Expedition)
                .WithMany()
                .HasForeignKey(h => h.ExpeditionId)
                .OnDelete(DeleteBehavior.Restrict);

            // PointOfInterest - Relationships (all optional FKs)
            modelBuilder.Entity<PointOfInterest>()
                .HasOne(p => p.GMMapHex)
                .WithMany(g => g.PointsOfInterest)
                .HasForeignKey(p => p.GMMapHexId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PointOfInterest>()
                .HasOne(p => p.TownMapHex)
                .WithMany(t => t.PointsOfInterest)
                .HasForeignKey(p => p.TownMapHexId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PointOfInterest>()
                .HasOne(p => p.DiscoveredByExpedition)
                .WithMany()
                .HasForeignKey(p => p.DiscoveredByExpeditionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}