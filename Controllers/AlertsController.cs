using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    [RequireLogin]
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public AlertsController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IActionResult> Index(bool? acknowledged, string? alertLevel)
        {
            var query = _context.ThreatAlerts
                .Include(a => a.Target)
                .Include(a => a.AcknowledgedByNavigation)
                .Include(a => a.AlertLevelNavigation)
                .AsQueryable();

            if (acknowledged.HasValue)
                query = query.Where(a => a.IsAcknowledged == (acknowledged.Value ? 1 : 0));

            if (!string.IsNullOrEmpty(alertLevel))
                query = query.Where(a => a.AlertLevelNavigation.Name == alertLevel);

            var alerts = await query.OrderByDescending(a => a.AlertId).ToListAsync();

            ViewBag.Acknowledged = acknowledged;
            ViewBag.AlertLevel = alertLevel;
            ViewBag.AlertLevels = await _context.AlertLevels.ToListAsync();

            return View(alerts);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var alert = await _context.ThreatAlerts
                .Include(a => a.Target)
                .Include(a => a.AcknowledgedByNavigation)
                .Include(a => a.AlertLevelNavigation)
                .FirstOrDefaultAsync(a => a.AlertId == id);

            if (alert == null) return NotFound();

            return View(alert);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acknowledge(int id)
        {
            var alert = await _context.ThreatAlerts
                .Include(a => a.Target)
                .FirstOrDefaultAsync(a => a.AlertId == id);

            if (alert == null) return NotFound();

            alert.IsAcknowledged = 1;
            alert.AcknowledgedBy = HttpContext.Session.GetUserId();
            alert.AcknowledgedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "System Alert",
                $"Alert for target {alert.Target?.TargetCode} acknowledged",
                "Info",
                HttpContext.Session.GetUserId(),
                alert.TargetId
            );

            TempData["SuccessMessage"] = "Alert acknowledged!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeAll()
        {
            var unacknowledgedAlerts = await _context.ThreatAlerts
                .Where(a => a.IsAcknowledged == 0)
                .ToListAsync();

            var userId = HttpContext.Session.GetUserId();
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var alert in unacknowledgedAlerts)
            {
                alert.IsAcknowledged = 1;
                alert.AcknowledgedBy = userId;
                alert.AcknowledgedAt = now;
            }

            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "System Alert",
                $"All alerts ({unacknowledgedAlerts.Count}) acknowledged",
                "Info",
                userId
            );

            TempData["SuccessMessage"] = $"{unacknowledgedAlerts.Count} alerts acknowledged!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var count = await _context.ThreatAlerts.CountAsync(a => a.IsAcknowledged == 0);
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent(int count = 5)
        {
            var alerts = await _context.ThreatAlerts
                .Include(a => a.Target)
                .Where(a => a.IsAcknowledged == 0)
                .OrderByDescending(a => a.AlertId)
                .Take(count)
                .Select(a => new
                {
                    a.AlertId,
                    AlertLevel = a.AlertLevelNavigation != null ? a.AlertLevelNavigation.Name : "Unknown",
                    a.Message,
                    a.CreatedAt,
                    TargetCode = a.Target != null ? a.Target.TargetCode : "Unknown"
                })
                .ToListAsync();

            return Json(alerts);
        }
    }
}