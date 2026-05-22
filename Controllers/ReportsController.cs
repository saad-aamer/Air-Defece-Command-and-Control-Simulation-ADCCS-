using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    [RequireLogin]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var viewModel = new ReportViewModel
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today,

                TotalTargetsDetected = await _context.Targets.CountAsync(),
                HostileEngagements = await _context.Targets.CountAsync(t => t.ClassificationNavigation.Name == "Hostile"),
                ActionsIssued = await _context.DefensiveActions.CountAsync(),
                AlertsGenerated = await _context.ThreatAlerts.CountAsync(),

                TargetsByType = await _context.Targets
                    .GroupBy(t => t.TargetTypeNavigation.Name)
                    .Select(g => new ChartDataPoint
                    {
                        Label = g.Key,
                        Value = g.Count(),
                        Color = g.Key == "Aircraft" ? "#0f3460" :
                                g.Key == "Missile" ? "#e94560" :
                                g.Key == "Drone" ? "#ffc107" : "#28a745"
                    })
                    .ToListAsync(),

                TargetsByClassification = await _context.Targets
                    .GroupBy(t => t.ClassificationNavigation.Name)
                    .Select(g => new ChartDataPoint
                    {
                        Label = g.Key,
                        Value = g.Count(),
                        Color = g.Key == "Hostile" ? "#e94560" :
                                g.Key == "Friendly" ? "#28a745" : "#ffc107"
                    })
                    .ToListAsync(),

                ActionsByType = await _context.DefensiveActions
                    .GroupBy(a => a.ActionTypeNavigation.Name)
                    .Select(g => new ChartDataPoint
                    {
                        Label = g.Key,
                        Value = g.Count(),
                        Color = "#0f3460"
                    })
                    .ToListAsync(),

                AlertsByLevel = await _context.ThreatAlerts
                    .GroupBy(a => a.AlertLevelNavigation.Name)
                    .Select(g => new ChartDataPoint
                    {
                        Label = g.Key,
                        Value = g.Count(),
                        Color = g.Key == "Critical" ? "#dc3545" :
                                g.Key == "High" ? "#e94560" :
                                g.Key == "Medium" ? "#ffc107" : "#28a745"
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Reports/Logs
        public async Task<IActionResult> Logs(string? eventType, string? severity, int page = 1)
        {
            var query = _context.MissionLogs
                .Include(l => l.User)
                .Include(l => l.Target)
                .Include(l => l.EventTypeNavigation)
                .Include(l => l.SeverityNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(l => l.EventTypeNavigation.Name == eventType);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(l => l.SeverityNavigation.Name == severity);

            var pageSize = 20;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var logs = await query
                .OrderByDescending(l => l.LogId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.EventType = eventType;
            ViewBag.Severity = severity;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            
            ViewBag.EventTypes = await _context.EventTypes.ToListAsync();
            ViewBag.Severities = await _context.SeverityLevels.ToListAsync();

            return View(logs);
        }

        // GET: Reports/MissionReport
        public async Task<IActionResult> MissionReport(string? startDateStr, string? endDateStr)
        {
            DateTime startDate;
            DateTime endDate;

            // Parse dates from query string or use defaults
            if (!string.IsNullOrEmpty(startDateStr) && DateTime.TryParse(startDateStr, out DateTime parsedStart))
            {
                startDate = parsedStart;
            }
            else
            {
                startDate = DateTime.Today.AddDays(-7);
            }

            if (!string.IsNullOrEmpty(endDateStr) && DateTime.TryParse(endDateStr, out DateTime parsedEnd))
            {
                endDate = parsedEnd;
            }
            else
            {
                endDate = DateTime.Today;
            }

            // Convert to strings for SQLite comparison
            var startStr = startDate.ToString("yyyy-MM-dd");
            var endStr = endDate.ToString("yyyy-MM-dd");

            var viewModel = new ReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,

                TotalTargetsDetected = await _context.Targets
                    .CountAsync(t => t.DetectedAt != null &&
                                     string.Compare(t.DetectedAt, startStr) >= 0 &&
                                     string.Compare(t.DetectedAt, endStr) <= 0),

                HostileEngagements = await _context.Targets
                    .CountAsync(t => t.ClassificationNavigation.Name == "Hostile" &&
                                     t.DetectedAt != null &&
                                     string.Compare(t.DetectedAt, startStr) >= 0),

                ActionsIssued = await _context.DefensiveActions
                    .CountAsync(a => a.IssuedAt != null &&
                                     string.Compare(a.IssuedAt, startStr) >= 0),

                AlertsGenerated = await _context.ThreatAlerts
                    .CountAsync(a => a.CreatedAt != null &&
                                     string.Compare(a.CreatedAt, startStr) >= 0),

                MissionLogs = await _context.MissionLogs
                    .Include(l => l.User)
                    .Include(l => l.Target)
                    .Include(l => l.EventTypeNavigation)
                    .Include(l => l.SeverityNavigation)
                    .Where(l => l.CreatedAt != null &&
                                string.Compare(l.CreatedAt, startStr) >= 0)
                    .OrderByDescending(l => l.LogId)
                    .Take(100)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}