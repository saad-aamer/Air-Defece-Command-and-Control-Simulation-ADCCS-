using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models;

public partial class SeverityLevel
{
    [Key]
    public int SeverityId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [InverseProperty("SeverityNavigation")]
    public virtual ICollection<MissionLog> MissionLogs { get; set; } = new List<MissionLog>();
}
