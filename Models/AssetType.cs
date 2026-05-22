using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADCCS_Web.Models
{
    public partial class AssetType
    {
        [Key]
        public int AssetTypeId { get; set; }

        public string Name { get; set; } = null!;

        [InverseProperty("AssetType")]
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
