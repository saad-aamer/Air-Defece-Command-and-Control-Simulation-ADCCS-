using ADCCS_Web.Models;

namespace ADCCS_Web.Models.ViewModels
{
    public class RadarViewModel
    {
        public List<Target> ActiveTargets { get; set; } = new List<Target>();
        public RadarConfiguration? Configuration { get; set; }
        public int RefreshInterval { get; set; } = 2000;
        public string UserRole { get; set; } = "Operator";
    }

    public class TargetDto
    {
        public int TargetId { get; set; }
        public string TargetCode { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string Classification { get; set; } = "Unknown";
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Speed { get; set; }
        public double Altitude { get; set; }
        public double Heading { get; set; }
    }
}