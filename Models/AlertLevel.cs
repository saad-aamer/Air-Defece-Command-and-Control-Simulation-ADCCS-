using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models;

public partial class AlertLevel
{
    [Key]
    public int AlertLevelId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [InverseProperty("AlertLevelNavigation")]
    public virtual ICollection<ThreatAlert> ThreatAlerts { get; set; } = new List<ThreatAlert>();
}
