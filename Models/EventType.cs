using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models;

public partial class EventType
{
    [Key]
    public int EventTypeId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [InverseProperty("EventTypeNavigation")]
    public virtual ICollection<MissionLog> MissionLogs { get; set; } = new List<MissionLog>();
}
