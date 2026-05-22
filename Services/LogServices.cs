using ADCCS_Web.Data;
using ADCCS_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogEventAsync(string eventType, string description, string severity = "Info",
            int? userId = null, int? targetId = null, int? actionId = null)
        {
            var eventTypeRecord = await _context.EventTypes.FirstOrDefaultAsync(e => e.Name == eventType);
            var severityRecord = await _context.SeverityLevels.FirstOrDefaultAsync(s => s.Name == severity);

            var log = new MissionLog
            {
                EventTypeId = eventTypeRecord?.EventTypeId ?? 1,
                Description = description,
                SeverityId = severityRecord?.SeverityId,
                UserId = userId,
                TargetId = targetId,
                ActionId = actionId,
                MissionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _context.MissionLogs.Add(log);
           int v = await _context.SaveChangesAsync();
        }

        public async Task<List<MissionLog>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null,
            int? eventTypeId = null, int limit = 100)
        {
            var query = _context.MissionLogs.AsQueryable();

            if (eventTypeId.HasValue)
            {
                query = query.Where(l => l.EventTypeId == eventTypeId.Value);
            }

            return await query
                .OrderByDescending(l => l.LogId)
                .Take(limit)
                .Include(l => l.User)
                .Include(l => l.Target)
                .Include(l => l.EventTypeNavigation)
                .Include(l => l.SeverityNavigation)
                .ToListAsync();
        }
    }
}