using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models;

public partial class TargetType
{
    [Key]
    public int TargetTypeId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [InverseProperty("TargetTypeNavigation")]
    public virtual ICollection<Target> Targets { get; set; } = new List<Target>();
}
