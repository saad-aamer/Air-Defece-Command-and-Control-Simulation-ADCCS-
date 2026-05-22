using ADCCS_Web.Models;

namespace ADCCS_Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Target Statistics
        public int TotalTargets { get; set; }
        public int ActiveTargets { get; set; }
        public int HostileTargets { get; set; }
        public int FriendlyTargets { get; set; }
        public int UnknownTargets { get; set; }

        // Action Statistics
        public int PendingActions { get; set; }
        public int CompletedActions { get; set; }

        // Alert Statistics
        public int ActiveAlerts { get; set; }

        // Recent Data Lists - initialized to avoid null reference
        public List<Target> RecentTargets { get; set; } = new List<Target>();
        public List<DefensiveAction> RecentActions { get; set; } = new List<DefensiveAction>();
        public List<ThreatAlert> ActiveThreatAlerts { get; set; } = new List<ThreatAlert>();

        // Radar Configuration - nullable
        public RadarConfiguration? RadarConfig { get; set; }
    }
}