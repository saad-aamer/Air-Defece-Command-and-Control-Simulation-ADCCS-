using ADCCS_Web.Models;
using ADCCS_Web.Models.ViewModels;

namespace ADCCS_Web.Services
{
    public interface IRadarService
    {
        Task<List<Target>> GetActiveTargetsAsync();
        Task<Target> GenerateRandomTargetAsync(int? userId = null);
        Task<Target?> GetTargetByIdAsync(int targetId);
        Task UpdateTargetPositionAsync(int targetId, double posX, double posY, double heading);
        Task<bool> ClassifyTargetAsync(int targetId, string classification, int userId);
        Task<RadarConfiguration?> GetActiveConfigurationAsync();
        Task<bool> UpdateConfigurationAsync(RadarConfiguration config);
        List<TargetDto> SimulateTargetMovement(List<Target> targets);
    }
}