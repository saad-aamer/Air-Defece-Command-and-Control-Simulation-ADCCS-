using Microsoft.AspNetCore.Http;

namespace ADCCS_Web.Helpers
{
    public static class SessionHelper
    {
        // Session key constants
        public const string UserIdKey = "UserId";
        public const string UsernameKey = "Username";
        public const string RoleKey = "Role";
        public const string FullNameKey = "FullName";

        // Extension method to check if user is logged in
        public static bool IsLoggedIn(this ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString(UserIdKey));
        }

        // Extension method to get user ID
        public static int GetUserId(this ISession session)
        {
            var userId = session.GetString(UserIdKey);
            return string.IsNullOrEmpty(userId) ? 0 : int.Parse(userId);
        }

        // Extension method to get user role
        public static string GetUserRole(this ISession session)
        {
            return session.GetString(RoleKey) ?? "";
        }

        // Extension method to get username
        public static string GetUsername(this ISession session)
        {
            return session.GetString(UsernameKey) ?? "";
        }

        // Extension method to get full name
        public static string GetFullName(this ISession session)
        {
            return session.GetString(FullNameKey) ?? "";
        }

        // Extension method to set user session
        public static void SetUserSession(this ISession session, int userId, string username, string role, string fullName)
        {
            session.SetString(UserIdKey, userId.ToString());
            session.SetString(UsernameKey, username);
            session.SetString(RoleKey, role);
            session.SetString(FullNameKey, fullName);
        }

        // Extension method to clear user session
        public static void ClearUserSession(this ISession session)
        {
            session.Remove(UserIdKey);
            session.Remove(UsernameKey);
            session.Remove(RoleKey);
            session.Remove(FullNameKey);
        }
    }
}