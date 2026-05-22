using ADCCS_Web.Data;
using ADCCS_Web.Helpers;
using ADCCS_Web.Models;
using ADCCS_Web.Models.ViewModels;
using ADCCS_Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogService _logService;

        public AccountController(ApplicationDbContext context, IAuthService authService, ILogService logService)
        {
            _context = context;
            _authService = authService;
            _logService = logService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.IsLoggedIn())
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _context.Users
                    .Include(u => u.Role)   
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }

                if (user.IsActive != 1)
                {
                    ModelState.AddModelError("", "Your account has been deactivated");
                    return View(model);
                }

                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }

                HttpContext.Session.SetString(SessionHelper.UserIdKey, user.UserId.ToString());
                HttpContext.Session.SetString(SessionHelper.UsernameKey, user.Username);
                HttpContext.Session.SetString(SessionHelper.RoleKey, user.Role.RoleName);
                HttpContext.Session.SetString(SessionHelper.FullNameKey, user.FullName);

                try
                {
                    await _logService.LogEventAsync(
                        "User Login",
                        $"User {user.Username} logged in successfully",
                        "Info",
                        (int)user.UserId
                    );
                }
                catch { }

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Login failed: {ex.Message}");
                return View(model);
            }
        }



        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetUserId();
            var username = HttpContext.Session.GetUsername();

            if (userId > 0)
            {
                try
                {
                    await _logService.LogEventAsync(
                        "User Login",
                        $"User {username} logged out",
                        "Info",
                        userId
                    );
                }
                catch { }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}