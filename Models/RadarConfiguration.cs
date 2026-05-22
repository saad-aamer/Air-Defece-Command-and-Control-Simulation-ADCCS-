using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Models;

public partial class RadarConfiguration
{
    [Key]
    public int ConfigId { get; set; }

    public string ConfigName { get; set; } = null!;

    public double? RadarRange { get; set; }

    public int? ScanInterval { get; set; }

    public int? MaxTargets { get; set; }

    public int? AutoClassification { get; set; }

    public double? AlertThreshold { get; set; }

    public int? IsActive { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("RadarConfigurations")]
    public virtual User? UpdatedByNavigation { get; set; }
}
