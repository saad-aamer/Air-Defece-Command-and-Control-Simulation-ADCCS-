using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Models;

[Index("StatusId", Name = "idx_actions_status")]
public partial class DefensiveAction
{
    [Key]
    public int ActionId { get; set; }

    public int TargetId { get; set; }

    public int ActionTypeId { get; set; }

    public int IssuedBy { get; set; }

    public string? IssuedAt { get; set; }

    public int? StatusId { get; set; }

    public string? Notes { get; set; }

    public string? CompletedAt { get; set; }

    public int? AssetId { get; set; }

    [ForeignKey("AssetId")]
    [InverseProperty("DefensiveActions")]
    public virtual Asset? Asset { get; set; }

    [ForeignKey("IssuedBy")]
    [InverseProperty("DefensiveActions")]
    public virtual User IssuedByNavigation { get; set; } = null!;

    [InverseProperty("Action")]
    public virtual ICollection<MissionLog> MissionLogs { get; set; } = new List<MissionLog>();

    [ForeignKey("TargetId")]
    [InverseProperty("DefensiveActions")]
    public virtual Target Target { get; set; } = null!;

    [ForeignKey("ActionTypeId")]
    [InverseProperty("DefensiveActions")]
    public virtual ActionType ActionTypeNavigation { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("DefensiveActions")]
    public virtual ActionStatus? StatusNavigation { get; set; }
}
