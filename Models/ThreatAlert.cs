using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Models;

[Index("IsAcknowledged", Name = "idx_alerts_acknowledged")]
public partial class ThreatAlert
{
    [Key]
    public int AlertId { get; set; }

    public int TargetId { get; set; }

    public int AlertLevelId { get; set; }

    public string Message { get; set; } = null!;

    public int? IsAcknowledged { get; set; }

    public int? AcknowledgedBy { get; set; }

    public string? CreatedAt { get; set; }

    public string? AcknowledgedAt { get; set; }

    [ForeignKey("AcknowledgedBy")]
    [InverseProperty("ThreatAlerts")]
    public virtual User? AcknowledgedByNavigation { get; set; }

    [ForeignKey("TargetId")]
    [InverseProperty("ThreatAlerts")]
    public virtual Target Target { get; set; } = null!;

    [ForeignKey("AlertLevelId")]
    [InverseProperty("ThreatAlerts")]
    public virtual AlertLevel AlertLevelNavigation { get; set; } = null!;
}
