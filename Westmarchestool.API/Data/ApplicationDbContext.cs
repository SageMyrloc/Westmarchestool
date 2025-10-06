using Microsoft.EntityFrameworkCore;
using Westmarchestool.API.Models;

namespace Westmarchestool.API.Data
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
        }
    }
}