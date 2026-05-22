using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Models;
using ADCCS_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    [RequireLogin]
    public class TargetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public TargetsController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Targets
        public async Task<IActionResult> Index(string searchString, int? classificationId, int? targetTypeId, bool? activeOnly)
        {
            var query = _context.Targets.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.TargetCode.Contains(searchString));
            }

            if (classificationId.HasValue)
            {
                query = query.Where(t => t.ClassificationId == classificationId);
            }

            if (targetTypeId.HasValue)
            {
                query = query.Where(t => t.TargetTypeId == targetTypeId);
            }

            if (activeOnly == true)
            {
                query = query.Where(t => t.IsActive == 1);
            }

            var targets = await query
                .OrderByDescending(t => t.TargetId)
                .Include(t => t.DetectedByNavigation)
                .Include(t => t.TargetTypeNavigation)
                .Include(t => t.ClassificationNavigation)
                .ToListAsync();

            // Pass filter values to view
            ViewBag.SearchString = searchString;
            ViewBag.ClassificationId = classificationId;
            ViewBag.TargetTypeId = targetTypeId;
            ViewBag.ActiveOnly = activeOnly;

            ViewBag.Classifications = await _context.Classifications.ToListAsync();
            ViewBag.TargetTypes = await _context.TargetTypes.ToListAsync();

            return View(targets);
        }

        // GET: Targets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var target = await _context.Targets
                .Include(t => t.DetectedByNavigation)
                .Include(t => t.TargetTypeNavigation)
                .Include(t => t.ClassificationNavigation)
                .Include(t => t.DefensiveActions)
                    .ThenInclude(a => a.IssuedByNavigation)
                .Include(t => t.DefensiveActions)
                    .ThenInclude(a => a.ActionTypeNavigation)
                .Include(t => t.DefensiveActions)
                    .ThenInclude(a => a.StatusNavigation)
                .Include(t => t.ThreatAlerts)
                    .ThenInclude(a => a.AlertLevelNavigation)
                .FirstOrDefaultAsync(t => t.TargetId == id);

            if (target == null)
            {
                return NotFound();
            }

            return View(target);
        }

        // GET: Targets/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Classifications = await _context.Classifications.ToListAsync();
            ViewBag.TargetTypes = await _context.TargetTypes.ToListAsync();
            return View();
        }

        // POST: Targets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Target target)
        {
            // Generate target code if not provided
            if (string.IsNullOrEmpty(target.TargetCode))
            {
                target.TargetCode = $"TGT-{DateTime.Now:HHmmss}-{new Random().Next(100, 999)}";
            }

            target.DetectedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            target.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            target.IsActive = 1;
            target.DetectedBy = HttpContext.Session.GetUserId();

            _context.Add(target);
            await _context.SaveChangesAsync();

            var classification = await _context.Classifications.FindAsync(target.ClassificationId);
            var targetType = await _context.TargetTypes.FindAsync(target.TargetTypeId);

            // Log the event
            await _logService.LogEventAsync(
                "Target Detected",
                $"New target created: {target.TargetCode} ({targetType?.Name})",
                classification?.Name == "Hostile" ? "Warning" : "Info",
                HttpContext.Session.GetUserId(),
                (int)target.TargetId
            );

            // Create alert if hostile
            if (classification?.Name == "Hostile")
            {
                var alertLevel = await _context.AlertLevels.FirstOrDefaultAsync(a => a.Name == "High");
                var alert = new ThreatAlert
                {
                    TargetId = (int)target.TargetId,
                    AlertLevelId = alertLevel?.AlertLevelId ?? 1,
                    Message = $"Hostile target detected: {target.TargetCode}",
                    IsAcknowledged = 0,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                _context.ThreatAlerts.Add(alert);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Target created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Targets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Target target)
        {
            if (id != target.TargetId) return NotFound();

            var existingTarget = await _context.Targets
                .Include(t => t.ClassificationNavigation)
                .FirstOrDefaultAsync(t => t.TargetId == id);
            if (existingTarget == null) return NotFound();

            var oldClassificationName = existingTarget.ClassificationNavigation?.Name;

            existingTarget.TargetCode = target.TargetCode;
            existingTarget.TargetTypeId = target.TargetTypeId;
            existingTarget.ClassificationId = target.ClassificationId;
            existingTarget.PositionX = target.PositionX;
            existingTarget.PositionY = target.PositionY;
            existingTarget.Speed = target.Speed;
            existingTarget.Altitude = target.Altitude;
            existingTarget.Heading = target.Heading;
            existingTarget.IsActive = target.IsActive;
            existingTarget.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();

            var newClassification = await _context.Classifications.FindAsync(target.ClassificationId);

            if (oldClassificationName != newClassification?.Name)
            {
                await _logService.LogEventAsync(
                    "Classification Changed",
                    $"Target {target.TargetCode} reclassified from {oldClassificationName} to {newClassification?.Name}",
                    newClassification?.Name == "Hostile" ? "Warning" : "Info",
                    HttpContext.Session.GetUserId(),
                    (int)target.TargetId
                );
            }

            TempData["SuccessMessage"] = "Target updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var target = await _context.Targets.FirstOrDefaultAsync(t => t.TargetId == id);
            if (target != null)
            {
                target.IsActive = 0;
                target.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _context.SaveChangesAsync();

                await _logService.LogEventAsync(
                    "Target Detected",
                    $"Target {target.TargetCode} removed from tracking",
                    "Info",
                    HttpContext.Session.GetUserId(),
                    id
                );

                TempData["SuccessMessage"] = "Target removed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Targets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var target = await _context.Targets
                .Include(t => t.DetectedByNavigation)
                .Include(t => t.TargetTypeNavigation)
                .Include(t => t.ClassificationNavigation)
                .FirstOrDefaultAsync(t => t.TargetId == id);

            if (target == null)
            {
                return NotFound();
            }

            return View(target);
        }

       

        // POST: Targets/Classify (AJAX)
        [HttpPost]
        public async Task<IActionResult> Classify(int targetId, int classificationId)
        {
            var target = await _context.Targets
                .Include(t => t.ClassificationNavigation)
                .FirstOrDefaultAsync(t => t.TargetId == targetId);

            if (target == null)
            {
                return Json(new { success = false, message = "Target not found" });
            }

            var newClassification = await _context.Classifications.FindAsync(classificationId);
            if (newClassification == null)
            {
                return Json(new { success = false, message = "Invalid classification" });
            }

            var oldClassificationName = target.ClassificationNavigation?.Name;
            target.ClassificationId = classificationId;
            target.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "Classification Changed",
                $"Target {target.TargetCode} reclassified from {oldClassificationName} to {newClassification.Name}",
                newClassification.Name == "Hostile" ? "Warning" : "Info",
                HttpContext.Session.GetUserId(),
                targetId
            );

            return Json(new { success = true });
        }

        private bool TargetExists(long id)
        {
            return _context.Targets.Any(e => e.TargetId == id);
        }
    }
}