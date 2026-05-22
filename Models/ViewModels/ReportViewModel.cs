using ADCCS_Web.Models;

namespace ADCCS_Web.Models.ViewModels
{
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;

        // Summary Statistics
        public int TotalTargetsDetected { get; set; }
        public int HostileEngagements { get; set; }
        public int ActionsIssued { get; set; }
        public int AlertsGenerated { get; set; }

        // Chart Data - initialized to avoid null
        public List<ChartDataPoint> TargetsByType { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> TargetsByClassification { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> ActionsByType { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> AlertsByLevel { get; set; } = new List<ChartDataPoint>();
        public List<DailyActivityData> DailyActivity { get; set; } = new List<DailyActivityData>();

        // Mission Logs
        public List<MissionLog> MissionLogs { get; set; } = new List<MissionLog>();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = "#000000";
    }

    public class DailyActivityData
    {
        public string Date { get; set; } = string.Empty;
        public int Targets { get; set; }
        public int Actions { get; set; }
        public int Alerts { get; set; }
    }
}