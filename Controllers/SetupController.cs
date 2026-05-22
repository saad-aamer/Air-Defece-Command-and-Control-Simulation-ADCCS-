using ADCCS_Web.Data;
using ADCCS_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ADCCS_Web.Controllers
{
    public class SetupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SetupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Setup/CreateDefaultUsers
        public async Task<IActionResult> CreateDefaultUsers()
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><style>");
            html.AppendLine("body { font-family: Arial; padding: 40px; background: #f5f5f5; }");
            html.AppendLine(".container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine("h1 { color: #0f3460; }");
            html.AppendLine(".success { color: green; padding: 10px; background: #d4edda; border-radius: 5px; margin: 10px 0; }");
            html.AppendLine(".error { color: red; padding: 10px; background: #f8d7da; border-radius: 5px; margin: 10px 0; }");
            html.AppendLine(".info { color: orange; padding: 10px; background: #fff3cd; border-radius: 5px; margin: 10px 0; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }");
            html.AppendLine("th { background: #0f3460; color: white; }");
            html.AppendLine(".btn { display: inline-block; padding: 12px 24px; background: #0f3460; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }");
            html.AppendLine(".btn:hover { background: #1a1a2e; }");
            html.AppendLine("</style></head><body><div class='container'>");
            html.AppendLine("<h1>🛡️ Air Defence System - Database Setup</h1>");

            try
            {
                // Step 1: Clear dependent tables first
                html.AppendLine("<h3>Step 1: Cleaning existing data...</h3>");

                var alertsDeleted = await _context.ThreatAlerts.ExecuteDeleteAsync();
                html.AppendLine($"<div class='info'>✓ Deleted {alertsDeleted} threat alerts</div>");

                var logsDeleted = await _context.MissionLogs.ExecuteDeleteAsync();
                html.AppendLine($"<div class='info'>✓ Deleted {logsDeleted} mission logs</div>");

                var actionsDeleted = await _context.DefensiveActions.ExecuteDeleteAsync();
                html.AppendLine($"<div class='info'>✓ Deleted {actionsDeleted} defensive actions</div>");

                var targetsDeleted = await _context.Targets.ExecuteDeleteAsync();
                html.AppendLine($"<div class='info'>✓ Deleted {targetsDeleted} targets</div>");

                var usersDeleted = await _context.Users.ExecuteDeleteAsync();
                html.AppendLine($"<div class='info'>✓ Deleted {usersDeleted} users</div>");

                var classificationsDeleted = await _context.Classifications.ExecuteDeleteAsync();
                var targetTypesDeleted = await _context.TargetTypes.ExecuteDeleteAsync();
                var alertLevelsDeleted = await _context.AlertLevels.ExecuteDeleteAsync();
                var actionTypesDeleted = await _context.ActionTypes.ExecuteDeleteAsync();
                var actionStatusesDeleted = await _context.ActionStatuses.ExecuteDeleteAsync();
                var severityLevelsDeleted = await _context.SeverityLevels.ExecuteDeleteAsync();
                var eventTypesDeleted = await _context.EventTypes.ExecuteDeleteAsync();
                var rolesDeleted = await _context.Roles.ExecuteDeleteAsync();

                html.AppendLine($"<div class='info'>✓ Deleted lookup tables</div>");

                await _context.SaveChangesAsync();

                html.AppendLine("<h3>Step 1.5: Creating Lookup Data...</h3>");
                _context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Operator" },
                    new Role { RoleName = "Commander" }
                );
                _context.Classifications.AddRange(
                    new Classification { Name = "Friendly" },
                    new Classification { Name = "Hostile" },
                    new Classification { Name = "Unknown" }
                );
                _context.TargetTypes.AddRange(
                    new TargetType { Name = "Aircraft" },
                    new TargetType { Name = "Missile" },
                    new TargetType { Name = "Drone" },
                    new TargetType { Name = "Helicopter" }
                );
                _context.ActionTypes.AddRange(
                    new ActionType { Name = "Scramble Jet" },
                    new ActionType { Name = "Launch SAM" },
                    new ActionType { Name = "Track Only" },
                    new ActionType { Name = "Engage" },
                    new ActionType { Name = "Ignore" }
                );
                _context.ActionStatuses.AddRange(
                    new ActionStatus { Name = "Pending" },
                    new ActionStatus { Name = "In Progress" },
                    new ActionStatus { Name = "Completed" },
                    new ActionStatus { Name = "Failed" }
                );
                _context.EventTypes.AddRange(
                    new EventType { Name = "User Login" },
                    new EventType { Name = "Target Detected" },
                    new EventType { Name = "Action Issued" },
                    new EventType { Name = "Classification Changed" },
                    new EventType { Name = "Configuration Changed" },
                    new EventType { Name = "System Alert" },
                    new EventType { Name = "Target Neutralized" }
                );
                _context.SeverityLevels.AddRange(
                    new SeverityLevel { Name = "Info" },
                    new SeverityLevel { Name = "Warning" },
                    new SeverityLevel { Name = "Critical" }
                );
                _context.AlertLevels.AddRange(
                    new AlertLevel { Name = "High" },
                    new AlertLevel { Name = "Medium" },
                    new AlertLevel { Name = "Critical" }
                );
                await _context.SaveChangesAsync();
                html.AppendLine($"<div class='info'>✓ Created lookup tables</div>");

                // Step 2: Create default users
                html.AppendLine("<h3>Step 2: Creating default users...</h3>");

                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
                var operatorRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Operator");
                var commanderRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Commander");

                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        RoleId = adminRole!.RoleId,
                        FullName = "System Administrator",
                        Email = "admin@airdefence.com",
                        IsActive = 1,
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    new User
                    {
                        Username = "operator1",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        RoleId = operatorRole!.RoleId,
                        FullName = "John Operator",
                        Email = "operator@airdefence.com",
                        IsActive = 1,
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    new User
                    {
                        Username = "commander1",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        RoleId = commanderRole!.RoleId,
                        FullName = "Jane Commander",
                        Email = "commander@airdefence.com",
                        IsActive = 1,
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };

                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();

                html.AppendLine("<div class='success'><strong>✅ Default Users Created Successfully!</strong></div>");

                // Step 3: Ensure radar configuration exists
                html.AppendLine("<h3>Step 3: Checking radar configuration...</h3>");

                var configExists = await _context.RadarConfigurations.AnyAsync();
                if (!configExists)
                {
                    var config = new RadarConfiguration
                    {
                        ConfigName = "Default Configuration",
                        RadarRange = 500.0,
                        ScanInterval = 2000,
                        MaxTargets = 50,
                        AutoClassification = 1,
                        AlertThreshold = 100.0,
                        IsActive = 1,
                        //UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    _context.RadarConfigurations.Add(config);
                    await _context.SaveChangesAsync();
                    html.AppendLine("<div class='success'>✓ Created default radar configuration</div>");
                }
                else
                {
                    html.AppendLine("<div class='info'>✓ Radar configuration already exists</div>");
                }

                // Step 4: Display credentials
                html.AppendLine("<h3>Step 4: Login Credentials</h3>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Username</th><th>Password</th><th>Role</th><th>Description</th></tr>");
                html.AppendLine("<tr><td><strong>admin</strong></td><td>Admin@123</td><td>Admin</td><td>Full system access</td></tr>");
                html.AppendLine("<tr><td><strong>operator1</strong></td><td>Admin@123</td><td>Operator</td><td>Can monitor radar, manage targets</td></tr>");
                html.AppendLine("<tr><td><strong>commander1</strong></td><td>Admin@123</td><td>Commander</td><td>Can issue defensive actions</td></tr>");
                html.AppendLine("</table>");

                html.AppendLine("<div class='success'>");
                html.AppendLine("<h3>✅ Setup Completed Successfully!</h3>");
                html.AppendLine("<p>All default accounts have been created with proper password hashing.</p>");
                html.AppendLine("<p>You can now login with any of the accounts above.</p>");
                html.AppendLine("</div>");
            }
            catch (Exception ex)
            {
                html.AppendLine($"<div class='error'>");
                html.AppendLine($"<h3>❌ Error During Setup</h3>");
                html.AppendLine($"<p><strong>Error:</strong> {ex.Message}</p>");
                html.AppendLine($"<p><strong>Stack Trace:</strong></p>");
                html.AppendLine($"<pre style='background: #f5f5f5; padding: 10px; border-radius: 5px; overflow-x: auto;'>{ex.StackTrace}</pre>");
                html.AppendLine("</div>");
            }

            html.AppendLine("<a href='/Account/Login' class='btn'>Go to Login Page</a>");
            html.AppendLine("<a href='/Setup/DatabaseStatus' class='btn' style='background: #28a745; margin-left: 10px;'>Check Database Status</a>");
            html.AppendLine("</div></body></html>");

            return Content(html.ToString(), "text/html");
        }

        // GET: /Setup/DatabaseStatus
        public async Task<IActionResult> DatabaseStatus()
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><style>");
            html.AppendLine("body { font-family: Arial; padding: 40px; background: #f5f5f5; }");
            html.AppendLine(".container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine("h1 { color: #0f3460; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }");
            html.AppendLine("th { background: #0f3460; color: white; }");
            html.AppendLine(".btn { display: inline-block; padding: 12px 24px; background: #0f3460; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }");
            html.AppendLine("</style></head><body><div class='container'>");
            html.AppendLine("<h1>📊 Database Status</h1>");

            try
            {
                var userCount = await _context.Users.CountAsync();
                var targetCount = await _context.Targets.CountAsync();
                var actionCount = await _context.DefensiveActions.CountAsync();
                var alertCount = await _context.ThreatAlerts.CountAsync();
                var logCount = await _context.MissionLogs.CountAsync();
                var configCount = await _context.RadarConfigurations.CountAsync();

                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Table</th><th>Record Count</th></tr>");
                html.AppendLine($"<tr><td>Users</td><td>{userCount}</td></tr>");
                html.AppendLine($"<tr><td>Targets</td><td>{targetCount}</td></tr>");
                html.AppendLine($"<tr><td>Defensive Actions</td><td>{actionCount}</td></tr>");
                html.AppendLine($"<tr><td>Threat Alerts</td><td>{alertCount}</td></tr>");
                html.AppendLine($"<tr><td>Mission Logs</td><td>{logCount}</td></tr>");
                html.AppendLine($"<tr><td>Radar Configurations</td><td>{configCount}</td></tr>");
                html.AppendLine("</table>");

                if (userCount > 0)
                {
                    var users = await _context.Users.Include(u => u.Role).ToListAsync();
                    html.AppendLine("<h3>Current Users</h3>");
                    html.AppendLine("<table>");
                    html.AppendLine("<tr><th>Username</th><th>Role</th><th>Full Name</th><th>Active</th></tr>");
                    foreach (var user in users)
                    {
                        html.AppendLine($"<tr><td>{user.Username}</td><td>{user.Role?.RoleName}</td><td>{user.FullName}</td><td>{(user.IsActive == 1 ? "✓" : "✗")}</td></tr>");
                    }
                    html.AppendLine("</table>");
                }
            }
            catch (Exception ex)
            {
                html.AppendLine($"<p style='color: red;'>Error: {ex.Message}</p>");
            }

            html.AppendLine("<a href='/Setup/CreateDefaultUsers' class='btn'>Reset Database</a>");
            html.AppendLine("<a href='/Account/Login' class='btn' style='background: #28a745; margin-left: 10px;'>Go to Login</a>");
            html.AppendLine("</div></body></html>");

            return Content(html.ToString(), "text/html");
        }
    }
}