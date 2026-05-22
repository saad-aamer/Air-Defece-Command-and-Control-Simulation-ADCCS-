using ADCCS_Web.Models;

namespace ADCCS_Web.Services
{
    public interface ILogService
    {
        Task LogEventAsync(string eventType, string description, string severity = "Info",
            int? userId = null, int? targetId = null, int? actionId = null);
        Task<List<MissionLog>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null,
            int? eventTypeId = null, int limit = 100);
    }
}