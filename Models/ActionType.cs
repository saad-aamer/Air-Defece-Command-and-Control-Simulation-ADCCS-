using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models;

public partial class ActionType
{
    [Key]
    public int ActionTypeId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [InverseProperty("ActionTypeNavigation")]
    public virtual ICollection<DefensiveAction> DefensiveActions { get; set; } = new List<DefensiveAction>();
}
