using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ADCCS_Web.Models;

namespace ADCCS_Web.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Target> Targets { get; set; } = null!;
        public virtual DbSet<DefensiveAction> DefensiveActions { get; set; } = null!;
        public virtual DbSet<MissionLog> MissionLogs { get; set; } = null!;
        public virtual DbSet<RadarConfiguration> RadarConfigurations { get; set; } = null!;
        public virtual DbSet<ThreatAlert> ThreatAlerts { get; set; } = null!;
        public virtual DbSet<Asset> Assets { get; set; } = null!;
        public virtual DbSet<AssetType> AssetTypes { get; set; } = null!;
        
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<ActionType> ActionTypes { get; set; } = null!;
        public virtual DbSet<ActionStatus> ActionStatuses { get; set; } = null!;
        public virtual DbSet<AlertLevel> AlertLevels { get; set; } = null!;
        public virtual DbSet<EventType> EventTypes { get; set; } = null!;
        public virtual DbSet<SeverityLevel> SeverityLevels { get; set; } = null!;
        public virtual DbSet<TargetType> TargetTypes { get; set; } = null!;
        public virtual DbSet<Classification> Classifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity => { entity.ToTable("Roles"); });
            modelBuilder.Entity<ActionType>(entity => { entity.ToTable("ActionTypes"); });
            modelBuilder.Entity<ActionStatus>(entity => { entity.ToTable("ActionStatuses"); });
            modelBuilder.Entity<AlertLevel>(entity => { entity.ToTable("AlertLevels"); });
            modelBuilder.Entity<EventType>(entity => { entity.ToTable("EventTypes"); });
            modelBuilder.Entity<SeverityLevel>(entity => { entity.ToTable("SeverityLevels"); });
            modelBuilder.Entity<TargetType>(entity => { entity.ToTable("TargetTypes"); });
            modelBuilder.Entity<Classification>(entity => { entity.ToTable("Classifications"); });
            modelBuilder.Entity<AssetType>(entity => { entity.ToTable("AssetTypes"); });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Assets");
                entity.HasKey(e => e.AssetId);
                
                entity.HasOne(d => d.AssetType)
                    .WithMany(p => p.Assets)
                    .HasForeignKey(d => d.AssetTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Target>(entity =>
            {
                entity.ToTable("Targets");
                entity.HasKey(e => e.TargetId);
                entity.HasIndex(e => e.TargetCode).IsUnique();

                entity.HasOne(d => d.DetectedByNavigation)
                    .WithMany(p => p.Targets)
                    .HasForeignKey(d => d.DetectedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DefensiveAction>(entity =>
            {
                entity.ToTable("DefensiveActions");
                entity.HasKey(e => e.ActionId);

                entity.HasOne(d => d.Target)
                    .WithMany(p => p.DefensiveActions)
                    .HasForeignKey(d => d.TargetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.IssuedByNavigation)
                    .WithMany(p => p.DefensiveActions)
                    .HasForeignKey(d => d.IssuedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MissionLog>(entity =>
            {
                entity.ToTable("MissionLogs");
                entity.HasKey(e => e.LogId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.MissionLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Target)
                    .WithMany(p => p.MissionLogs)
                    .HasForeignKey(d => d.TargetId)
                    .OnDelete(DeleteBehavior.SetNull);

                //entity.HasOne(d => d.DefensiveAction)
                //    .WithMany()
                //    .HasForeignKey(d => d.ActionId)
                //    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<RadarConfiguration>(entity =>
            {
                entity.ToTable("RadarConfigurations");
                entity.HasKey(e => e.ConfigId);
            });

            modelBuilder.Entity<ThreatAlert>(entity =>
            {
                entity.ToTable("ThreatAlerts");
                entity.HasKey(e => e.AlertId);

                entity.HasOne(d => d.Target)
                    .WithMany(p => p.ThreatAlerts)
                    .HasForeignKey(d => d.TargetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.AcknowledgedByNavigation)
                    .WithMany(p => p.ThreatAlerts)
                    .HasForeignKey(d => d.AcknowledgedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}