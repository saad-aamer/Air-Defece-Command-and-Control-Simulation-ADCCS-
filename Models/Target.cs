using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Models;

[Index("TargetCode", IsUnique = true)]
[Index("IsActive", Name = "idx_targets_active")]
[Index("ClassificationId", Name = "idx_targets_classification")]
[Index("TargetCode", Name = "idx_targets_code")]
public partial class Target
{
    [Key]
    public int TargetId { get; set; }

    public string TargetCode { get; set; } = null!;

    public int TargetTypeId { get; set; }

    public int? ClassificationId { get; set; }

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public double Speed { get; set; }

    public double Altitude { get; set; }

    public double Heading { get; set; }

    public string? DetectedAt { get; set; }

    public string? LastUpdated { get; set; }

    public int? IsActive { get; set; }

    public int? DetectedBy { get; set; }

    public int? AssetId { get; set; }

    [ForeignKey("AssetId")]
    [InverseProperty("Targets")]
    public virtual Asset? Asset { get; set; }

    [InverseProperty("Target")]
    public virtual ICollection<DefensiveAction> DefensiveActions { get; set; } = new List<DefensiveAction>();

    [ForeignKey("DetectedBy")]
    [InverseProperty("Targets")]
    public virtual User? DetectedByNavigation { get; set; }

    [InverseProperty("Target")]
    public virtual ICollection<MissionLog> MissionLogs { get; set; } = new List<MissionLog>();

    [InverseProperty("Target")]
    public virtual ICollection<ThreatAlert> ThreatAlerts { get; set; } = new List<ThreatAlert>();

    [ForeignKey("TargetTypeId")]
    [InverseProperty("Targets")]
    public virtual TargetType TargetTypeNavigation { get; set; } = null!;

    [ForeignKey("ClassificationId")]
    [InverseProperty("Targets")]
    public virtual Classification? ClassificationNavigation { get; set; }
}
