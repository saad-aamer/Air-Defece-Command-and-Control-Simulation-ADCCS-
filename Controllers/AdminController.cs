using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Models;
using ADCCS_Web.Models.ViewModels;
using ADCCS_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogService _logService;

        public AdminController(ApplicationDbContext context, IAuthService authService, ILogService logService)
        {
            _context = context;
            _authService = authService;
            _logService = logService;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.Include(u => u.Role).OrderByDescending(u => u.UserId).ToListAsync();
            return View(users);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validate role
            var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == model.Role.Trim());
            if (roleRecord == null)
            {
                ModelState.AddModelError("Role", "Invalid role selected");
                return View(model);
            }

            // Check if username exists
            var exists = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (exists)
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username.Trim(),
                PasswordHash = _authService.HashPassword(model.Password),
                RoleId = roleRecord.RoleId,
                FullName = model.FullName.Trim(),
                Email = model.Email?.Trim(),
                IsActive = 1,
                //CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "Configuration Changed",
                $"New user created: {user.Username} ({roleRecord.RoleName})",
                "Info",
                HttpContext.Session.GetUserId()
            );

            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                UserId = (int)user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.RoleName,
                IsActive = user.IsActive == 1
            };

            return View(model);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, EditUserViewModel model)
        {
            if (id != model.UserId) return NotFound();

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.Remove("Password");
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username && u.UserId != id);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == model.Role.Trim());
            if (roleRecord == null)
            {
                ModelState.AddModelError("Role", "Invalid role selected");
                return View(model);
            }

            user.Username = model.Username.Trim();
            user.FullName = model.FullName.Trim();
            user.Email = model.Email?.Trim();
            user.RoleId = roleRecord.RoleId;
            user.IsActive = model.IsActive ? 1 : 0;

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = _authService.HashPassword(model.Password);
            }

            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "Configuration Changed",
                $"User updated: {user.Username}",
                "Info",
                HttpContext.Session.GetUserId()
            );

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            if (user.UserId == HttpContext.Session.GetUserId())
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(Users));
            }

            user.IsActive = 0;
            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "Configuration Changed",
                $"User deactivated: {user.Username}",
                "Warning",
                HttpContext.Session.GetUserId()
            );

            TempData["SuccessMessage"] = "User deactivated successfully!";
            return RedirectToAction(nameof(Users));
        }

        
        // GET: Admin/Configuration
        public async Task<IActionResult> Configuration()
        {
            var config = await _context.RadarConfigurations.FirstOrDefaultAsync(c => c.IsActive == 1);
            return View(config);
        }

        // POST: Admin/Configuration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Configuration(RadarConfiguration model)
        {
            var config = await _context.RadarConfigurations.FirstOrDefaultAsync(c => c.IsActive == 1);
            if (config == null)
            {
                TempData["ErrorMessage"] = "Configuration not found!";
                return View(model);
            }

            config.ConfigName = model.ConfigName;
            config.RadarRange = model.RadarRange;
            config.ScanInterval = model.ScanInterval;
            config.MaxTargets = model.MaxTargets;
            config.AutoClassification = model.AutoClassification;
            config.AlertThreshold = model.AlertThreshold;
            config.UpdatedBy = HttpContext.Session.GetUserId();
            //config.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "Configuration Changed",
                "Radar configuration updated",
                "Info",
                HttpContext.Session.GetUserId()
            );

            TempData["SuccessMessage"] = "Configuration updated successfully!";
            return RedirectToAction(nameof(Configuration));
        }
    }
}