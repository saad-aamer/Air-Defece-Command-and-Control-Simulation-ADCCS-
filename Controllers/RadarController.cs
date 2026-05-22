using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Models;
using ADCCS_Web.Models.ViewModels;
using ADCCS_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    [RequireLogin]
    public class RadarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRadarService _radarService;
        private readonly ILogService _logService;

        public async Task<IActionResult> DebugInfo()
        {
            var statuses = await _context.ActionStatuses.ToListAsync();
            var types = await _context.ActionTypes.ToListAsync();
            return Json(new { Statuses = statuses, Types = types });
        }

        public RadarController(ApplicationDbContext context, IRadarService radarService, ILogService logService)
        {
            _context = context;
            _radarService = radarService;
            _logService = logService;
        }

        public async Task<IActionResult> Monitor()
        {
            var config = await _radarService.GetActiveConfigurationAsync();
            var targets = await _radarService.GetActiveTargetsAsync();

            var viewModel = new RadarViewModel
            {
                ActiveTargets = targets,
                Configuration = config,
                RefreshInterval = config?.ScanInterval ?? 2000,
                UserRole = HttpContext.Session.GetUserRole()
            };

            ViewBag.ActionTypes = await _context.ActionTypes.ToListAsync();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetTargets()
        {
            var targets = await _radarService.GetActiveTargetsAsync();

            var dtos = targets.Select(t => new TargetDto
            {
                TargetId = (int)t.TargetId,
                TargetCode = t.TargetCode,
                TargetType = t.TargetTypeNavigation?.Name ?? "Unknown",
                Classification = t.ClassificationNavigation?.Name ?? "Unknown",
                PositionX = t.PositionX,
                PositionY = t.PositionY,
                Speed = t.Speed,
                Altitude = t.Altitude,
                Heading = t.Heading
            }).ToList();

            return Json(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTarget()
        {
            var userId = HttpContext.Session.GetUserId();
            var target = await _radarService.GenerateRandomTargetAsync(userId);

            // Fetch the generated target with navigation properties
            var generatedTarget = await _context.Targets
                .Include(t => t.TargetTypeNavigation)
                .Include(t => t.ClassificationNavigation)
                .FirstOrDefaultAsync(t => t.TargetId == target.TargetId);

            return Json(new
            {
                success = true,
                target = new TargetDto
                {
                    TargetId = (int)generatedTarget!.TargetId,
                    TargetCode = generatedTarget.TargetCode,
                    TargetType = generatedTarget.TargetTypeNavigation?.Name ?? "Unknown",
                    Classification = generatedTarget.ClassificationNavigation?.Name ?? "Unknown",
                    PositionX = target.PositionX,
                    PositionY = target.PositionY,
                    Speed = target.Speed,
                    Altitude = target.Altitude,
                    Heading = target.Heading
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> ClassifyTarget(int targetId, string classification)
        {
            var validClassifications = new[] { "Friendly", "Hostile", "Unknown" };
            if (!validClassifications.Contains(classification))
            {
                return Json(new { success = false, message = "Invalid classification" });
            }

            var userId = HttpContext.Session.GetUserId();
            var result = await _radarService.ClassifyTargetAsync(targetId, classification, userId);

            return Json(new { success = result });
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> RemoveTarget(int targetId)
        {
            var target = await _context.Targets.FirstOrDefaultAsync(t => t.TargetId == targetId);
            if (target == null)
            {
                return Json(new { success = false, message = "Target not found" });
            }

            target.IsActive = 0;
            target.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await _context.SaveChangesAsync();

            var userId = HttpContext.Session.GetUserId();
            await _logService.LogEventAsync(
                "Target Detected",
                $"Target {target.TargetCode} removed from tracking",
                "Info",
                userId,
                targetId
            );

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetAlertCount()
        {
            var count = await _context.ThreatAlerts.CountAsync(a => a.IsAcknowledged == 0);
            return Json(new { count });
        }
    }
}