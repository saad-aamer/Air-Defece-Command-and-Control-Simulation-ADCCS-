using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models;

public partial class Classification
{
    [Key]
    public int ClassificationId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [InverseProperty("ClassificationNavigation")]
    public virtual ICollection<Target> Targets { get; set; } = new List<Target>();
}
