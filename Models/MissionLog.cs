using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Models;

[Index("EventTypeId", Name = "idx_logs_eventtype")]
public partial class MissionLog
{
    [Key]
    public int LogId { get; set; }

    public string? MissionDate { get; set; }

    public int EventTypeId { get; set; }

    public string Description { get; set; } = null!;

    public int? SeverityId { get; set; }

    public int? UserId { get; set; }

    public int? TargetId { get; set; }

    public int? ActionId { get; set; }

    public string? CreatedAt { get; set; }

    public int? AssetId { get; set; }

    [ForeignKey("AssetId")]
    public virtual Asset? Asset { get; set; }

    [ForeignKey("ActionId")]
    [InverseProperty("MissionLogs")]
    public virtual DefensiveAction? Action { get; set; }

    [ForeignKey("TargetId")]
    [InverseProperty("MissionLogs")]
    public virtual Target? Target { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("MissionLogs")]
    public virtual User? User { get; set; }

    [ForeignKey("EventTypeId")]
    [InverseProperty("MissionLogs")]
    public virtual EventType EventTypeNavigation { get; set; } = null!;

    [ForeignKey("SeverityId")]
    [InverseProperty("MissionLogs")]
    public virtual SeverityLevel? SeverityNavigation { get; set; }
}
