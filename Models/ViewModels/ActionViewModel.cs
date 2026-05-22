using System.ComponentModel.DataAnnotations;

namespace ADCCS_Web.Models.ViewModels
{
    public class CreateActionViewModel
    {
        [Required]
        public long TargetId { get; set; }

        [Required]
        [Display(Name = "Action Type")]
        public string ActionType { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        // For display purposes
        public string TargetCode { get; set; }
        public string TargetClassification { get; set; }
    }
}