using ADCCS_Web.Models;

namespace ADCCS_Web.Services
{
    public interface IAuthService
    {
        Task<User?> ValidateUserAsync(string username, string password);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> CreateUserAsync(string username, string password, string role, string fullName, string? email);
        Task UpdateLastLoginAsync(int userId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}