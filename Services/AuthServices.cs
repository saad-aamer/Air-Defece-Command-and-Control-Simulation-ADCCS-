using ADCCS_Web.Data;
using ADCCS_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _context.Users
                .AsNoTracking()  // Don't track for validation
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive == 1);

            if (user == null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> CreateUserAsync(string username, string password, string role, string fullName, string? email)
        {
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Username == username);
                if (exists)
                    return false;

                var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == role.Trim());
                var roleId = roleRecord?.RoleId ?? 1;

                var user = new User
                {
                    Username = username.Trim(),
                    PasswordHash = HashPassword(password),
                    RoleId = roleId,
                    FullName = fullName.Trim(),
                    Email = email?.Trim(),
                    IsActive = 1,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the error (you can add logging here)
                System.Diagnostics.Debug.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            try
            {
                // Find the user without tracking
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
                {
                    // Update last login
                    user.LastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // Mark only LastLogin as modified
                    _context.Entry(user).Property(u => u.LastLogin).IsModified = true;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Don't throw - login should still succeed even if LastLogin update fails
                System.Diagnostics.Debug.WriteLine($"Warning: Could not update last login for user {userId}: {ex.Message}");
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password verification error: {ex.Message}");
                return false;
            }
        }
    }
}