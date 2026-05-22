using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models;

public partial class ActionStatus
{
    [Key]
    public int StatusId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [InverseProperty("StatusNavigation")]
    public virtual ICollection<DefensiveAction> DefensiveActions { get; set; } = new List<DefensiveAction>();
}
