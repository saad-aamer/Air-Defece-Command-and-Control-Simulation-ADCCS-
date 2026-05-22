using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    [RequireLogin]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalTargets = await _context.Targets.CountAsync(),
                ActiveTargets = await _context.Targets.CountAsync(t => t.IsActive == 1),
                HostileTargets = await _context.Targets.CountAsync(t => t.ClassificationNavigation.Name == "Hostile" && t.IsActive == 1),
                FriendlyTargets = await _context.Targets.CountAsync(t => t.ClassificationNavigation.Name == "Friendly" && t.IsActive == 1),
                UnknownTargets = await _context.Targets.CountAsync(t => t.ClassificationNavigation.Name == "Unknown" && t.IsActive == 1),
                PendingActions = await _context.DefensiveActions.CountAsync(a => a.StatusNavigation.Name == "Pending"),
                CompletedActions = await _context.DefensiveActions.CountAsync(a => a.StatusNavigation.Name == "Completed"),
                ActiveAlerts = await _context.ThreatAlerts.CountAsync(a => a.IsAcknowledged == 0),

                RecentTargets = await _context.Targets
                    .Include(t => t.TargetTypeNavigation)
                    .Include(t => t.ClassificationNavigation)
                    .Where(t => t.IsActive == 1)
                    .OrderByDescending(t => t.TargetId)
                    .Take(5)
                    .ToListAsync(),

                RecentActions = await _context.DefensiveActions
                    .Include(a => a.Target)
                    .Include(a => a.IssuedByNavigation)
                    .Include(a => a.ActionTypeNavigation)
                    .Include(a => a.StatusNavigation)
                    .OrderByDescending(a => a.ActionId)
                    .Take(5)
                    .ToListAsync(),

                ActiveThreatAlerts = await _context.ThreatAlerts
                    .Include(a => a.Target)
                    .Include(a => a.AlertLevelNavigation)
                    .Where(a => a.IsAcknowledged == 0)
                    .OrderByDescending(a => a.AlertId)
                    .Take(5)
                    .ToListAsync(),

                RadarConfig = await _context.RadarConfigurations.FirstOrDefaultAsync(c => c.IsActive == 1)
            };

            return View(viewModel);
        }
    }
}