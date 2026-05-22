using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ADCCS_Web.Models;

[Index("Username", IsUnique = true)]
[Index("Username", Name = "idx_users_username")]
public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }

    public int RoleId { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public Role Role { get; set; }   // ✅ navigation property

    public string FullName { get; set; }
    public string? Email { get; set; }
    public int? IsActive { get; set; }
    public string? CreatedAt { get; set; }
    public string? LastLogin { get; set; }

    [InverseProperty("IssuedByNavigation")]
    public virtual ICollection<DefensiveAction> DefensiveActions { get; set; } = new List<DefensiveAction>();

    [InverseProperty("DetectedByNavigation")]
    public virtual ICollection<Target> Targets { get; set; } = new List<Target>();

    [InverseProperty("User")]
    public virtual ICollection<MissionLog> MissionLogs { get; set; } = new List<MissionLog>();

    [InverseProperty("AcknowledgedByNavigation")]
    public virtual ICollection<ThreatAlert> ThreatAlerts { get; set; } = new List<ThreatAlert>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<RadarConfiguration> RadarConfigurations { get; set; } = new List<RadarConfiguration>();
}
