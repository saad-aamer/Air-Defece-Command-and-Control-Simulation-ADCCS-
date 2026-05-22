using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Models;
using ADCCS_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    [AuthorizeRole("Commander", "Admin")]
    public class ActionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public ActionsController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Actions
        public async Task<IActionResult> Index(int? statusId, int? actionTypeId)
        {
            var query = _context.DefensiveActions
                .Include(a => a.Target)
                .Include(a => a.IssuedByNavigation)
                .Include(a => a.ActionTypeNavigation)
                .Include(a => a.StatusNavigation)
                .AsQueryable();

            if (statusId.HasValue)
                query = query.Where(a => a.StatusId == statusId);

            if (actionTypeId.HasValue)
                query = query.Where(a => a.ActionTypeId == actionTypeId);

            var actions = await query.OrderByDescending(a => a.ActionId).ToListAsync();

            ViewBag.StatusId = statusId;
            ViewBag.ActionTypeId = actionTypeId;
            
            ViewBag.ActionStatuses = await _context.ActionStatuses.ToListAsync();
            ViewBag.ActionTypes = await _context.ActionTypes.ToListAsync();

            return View(actions);
        }

        // GET: Actions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var action = await _context.DefensiveActions
                .Include(a => a.Target)
                .Include(a => a.IssuedByNavigation)
                .Include(a => a.ActionTypeNavigation)
                .Include(a => a.StatusNavigation)
                .FirstOrDefaultAsync(a => a.ActionId == id);

            if (action == null) return NotFound();

            return View(action);
        }

        // GET: Actions/Create
        public async Task<IActionResult> Create(int? targetId)
        {
            try
            {
                var hostileClass = await _context.Classifications.FirstOrDefaultAsync(c => c.Name == "Hostile");
                var unknownClass = await _context.Classifications.FirstOrDefaultAsync(c => c.Name == "Unknown");

                var targets = await _context.Targets
                    .Include(t => t.TargetTypeNavigation)
                    .Where(t => t.IsActive == 1 && (t.ClassificationId == hostileClass.ClassificationId || t.ClassificationId == unknownClass.ClassificationId))
                    .ToListAsync();

                var targetList = targets.Select(t => new {
                    t.TargetId,
                    t.TargetCode,
                    TargetTypeName = t.TargetTypeNavigation?.Name ?? ""
                }).ToList();

                var assets = await _context.Assets
                    .Include(a => a.AssetType)
                    .Select(a => new {
                        a.AssetId,
                        a.AssetName,
                        AssetTypeName = a.AssetType.Name
                    }).ToListAsync();

                ViewBag.TargetsList = new SelectList(targets, "TargetId", "TargetCode", targetId);
                ViewBag.SelectedTargetId = targetId;
                ViewBag.ActionTypes = await _context.ActionTypes.ToListAsync();
                
                ViewBag.TargetsJson = System.Text.Json.JsonSerializer.Serialize(targetList);
                ViewBag.AssetsJson = System.Text.Json.JsonSerializer.Serialize(assets);

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading create form: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Actions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int TargetId, int ActionTypeId, int? AssetId, string? Notes)
        {
            try
            {
                var actionTypeRecord = await _context.ActionTypes.FindAsync(ActionTypeId);
                if (actionTypeRecord == null)
                {
                    TempData["ErrorMessage"] = "Invalid action type selected";
                    return RedirectToAction(nameof(Create), new { targetId = TargetId });
                }

                // Check if target exists
                var target = await _context.Targets.FirstOrDefaultAsync(t => t.TargetId == TargetId);
                if (target == null)
                {
                    TempData["ErrorMessage"] = "Target not found";
                    return RedirectToAction(nameof(Index));
                }

                var pendingStatus = await _context.ActionStatuses.FirstOrDefaultAsync(s => s.Name == "Pending");

                var action = new DefensiveAction
                {
                    TargetId = TargetId,
                    ActionTypeId = ActionTypeId,
                    IssuedBy = HttpContext.Session.GetUserId(),
                    IssuedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    StatusId = pendingStatus?.StatusId,
                    AssetId = AssetId,
                    Notes = Notes?.Trim()
                };

                _context.DefensiveActions.Add(action);
                await _context.SaveChangesAsync();

                // Log the action
                try
                {
                    await _logService.LogEventAsync(
                        "Action Issued",
                        $"Action '{actionTypeRecord.Name}' issued for target {target.TargetCode}",
                        "Warning",
                        HttpContext.Session.GetUserId(),
                        TargetId,
                        (int)action.ActionId
                    );
                }
                catch { }

                TempData["SuccessMessage"] = $"Action '{actionTypeRecord.Name}' issued successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating action: {ex.Message}";
                return RedirectToAction(nameof(Create), new { targetId = TargetId });
            }
        }

        // POST: Actions/CreateAjax (for Radar page)
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] ActionRequest request)
        {
            try
            {
                var actionTypeRecord = await _context.ActionTypes.FirstOrDefaultAsync(a => 
                    (!string.IsNullOrEmpty(request.ActionTypeName) && a.Name == request.ActionTypeName) || 
                    (request.ActionTypeId.HasValue && a.ActionTypeId == request.ActionTypeId.Value));
                    
                if (actionTypeRecord == null)
                {
                    return Json(new { success = false, message = "Invalid action type" });
                }

                var target = await _context.Targets.FirstOrDefaultAsync(t => t.TargetId == request.TargetId);
                if (target == null)
                {
                    return Json(new { success = false, message = "Target not found" });
                }

                var pendingStatus = await _context.ActionStatuses.FirstOrDefaultAsync(s => s.Name == "Pending");

                var action = new DefensiveAction
                {
                    TargetId = request.TargetId,
                    ActionTypeId = actionTypeRecord.ActionTypeId,
                    IssuedBy = HttpContext.Session.GetUserId(),
                    IssuedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    StatusId = pendingStatus?.StatusId,
                    AssetId = request.AssetId,
                    Notes = request.Notes?.Trim()
                };

                _context.DefensiveActions.Add(action);
                await _context.SaveChangesAsync();

                try
                {
                    await _logService.LogEventAsync(
                        "Action Issued",
                        $"Action '{actionTypeRecord.Name}' issued for target {target.TargetCode}",
                        "Warning",
                        HttpContext.Session.GetUserId(),
                        action.TargetId,
                        (int)action.ActionId
                    );
                }
                catch { }

                return Json(new { success = true, actionId = action.ActionId, message = $"Action '{actionTypeRecord.Name}' issued for {target.TargetCode}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Actions/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string statusName)
        {
            try
            {
                if (string.Equals(statusName, "complete", StringComparison.OrdinalIgnoreCase))
                {
                    statusName = "Completed";
                }

                var newStatusRecord = await _context.ActionStatuses.FirstOrDefaultAsync(s => s.Name == statusName);
                if (newStatusRecord == null)
                {
                    TempData["ErrorMessage"] = $"Invalid status: {statusName}";
                    return RedirectToAction(nameof(Index));
                }

                var action = await _context.DefensiveActions
                    .Include(a => a.Target)
                    .Include(a => a.ActionTypeNavigation)
                    .FirstOrDefaultAsync(a => a.ActionId == id);

                if (action == null)
                {
                    TempData["ErrorMessage"] = "Action not found";
                    return RedirectToAction(nameof(Index));
                }

                action.StatusId = newStatusRecord.StatusId;
                if (newStatusRecord.Name == "Completed" || newStatusRecord.Name == "Failed" || newStatusRecord.Name == "Complete")
                {
                    action.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // If the action completed successfully, remove the target from radar
                if (newStatusRecord.Name == "Completed" || newStatusRecord.Name == "Complete")
                {
                    var target = action.Target;
                    if (target != null && target.IsActive == 1)
                    {
                        target.IsActive = 0;
                        target.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        await _context.SaveChangesAsync();

                        try
                        {
                            await _logService.LogEventAsync(
                                "Target Destroyed",
                                $"Target {target.TargetCode} was destroyed by action '{action.ActionTypeNavigation?.Name}' (ActionId: {action.ActionId})",
                                "Warning",
                                HttpContext.Session.GetUserId(),
                                (int)target.TargetId,
                                (int)action.ActionId
                            );
                        }
                        catch { }

                        TempData["SuccessMessage"] = "Target Destroyed";
                        return RedirectToAction(nameof(Index));
                    }
                }

                await _context.SaveChangesAsync();

                try
                {
                    await _logService.LogEventAsync(
                        "Action Issued",
                        $"Action '{action.ActionTypeNavigation?.Name}' for target {action.Target?.TargetCode} status changed to {newStatusRecord.Name}",
                        "Info",
                        HttpContext.Session.GetUserId(),
                        action.TargetId,
                        id
                    );
                }
                catch { }

                TempData["SuccessMessage"] = "Action status updated!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating status: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    public class ActionRequest
    {
        public int TargetId { get; set; }
        public int? ActionTypeId { get; set; }
        public string? ActionTypeName { get; set; }
        public int? AssetId { get; set; }
        public string? Notes { get; set; }
    }
}