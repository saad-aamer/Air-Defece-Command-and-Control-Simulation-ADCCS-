using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models
{
    public partial class Asset
    {
        [Key]
        public int AssetId { get; set; }

        public string AssetName { get; set; } = null!;

        public int AssetTypeId { get; set; }

        public double? MaxSpeed { get; set; }

        public double? MaxRange { get; set; }

        [ForeignKey("AssetTypeId")]
        [InverseProperty("Assets")]
        public virtual AssetType AssetType { get; set; } = null!;

        [InverseProperty("Asset")]
        public virtual ICollection<DefensiveAction> DefensiveActions { get; set; } = new List<DefensiveAction>();
        
        [InverseProperty("Asset")]
        public virtual ICollection<Target> Targets { get; set; } = new List<Target>();
    }
}
