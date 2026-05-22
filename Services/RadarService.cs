using ADCCS_Web.Data;
using ADCCS_Web.Models;
using ADCCS_Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ADCCS_Web.Services
{
    public class RadarService : IRadarService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private static readonly Random _random = new Random();

        public RadarService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<List<Target>> GetActiveTargetsAsync()
        {
            var targets = await _context.Targets
                .Include(t => t.TargetTypeNavigation)
                .Include(t => t.ClassificationNavigation)
                .Where(t => t.IsActive == 1)
                .OrderByDescending(t => t.TargetId)
                .ToListAsync();

            var now = DateTime.Now;
            var config = await GetActiveConfigurationAsync();
            double maxRange = config?.RadarRange ?? 500.0;
            bool updated = false;

            foreach (var target in targets)
            {
                if (DateTime.TryParse(target.LastUpdated, out DateTime lastUpdated))
                {
                    var seconds = (now - lastUpdated).TotalSeconds;
                    if (seconds > 0)
                    {
                        var headingRad = target.Heading * Math.PI / 180.0;
                        var visualSpeedMultiplier = 50.0;
                        var moveDistance = (target.Speed / 3600.0) * seconds * visualSpeedMultiplier;

                        target.PositionX += Math.Cos(headingRad) * moveDistance;
                        target.PositionY += Math.Sin(headingRad) * moveDistance;

                        double distance = Math.Sqrt(target.PositionX * target.PositionX + target.PositionY * target.PositionY);
                        if (distance > maxRange)
                        {
                            target.Heading = (target.Heading + 180) % 360;
                            double ratio = (maxRange - 2) / distance;
                            target.PositionX *= ratio;
                            target.PositionY *= ratio;
                        }

                        target.LastUpdated = now.ToString("yyyy-MM-dd HH:mm:ss");
                        updated = true;
                    }
                }
                else
                {
                    target.LastUpdated = now.ToString("yyyy-MM-dd HH:mm:ss");
                    updated = true;
                }
            }

            if (updated)
            {
                await _context.SaveChangesAsync();
            }

            return targets;
        }

        public async Task<Target> GenerateRandomTargetAsync(int? userId = null)
        {
            var config = await GetActiveConfigurationAsync();
            var radarRange = config?.RadarRange ?? 500.0;

            var targetTypes = await _context.TargetTypes.ToListAsync();
            var classifications = await _context.Classifications.ToListAsync();

            var angle = _random.NextDouble() * 2 * Math.PI;
            var distance = radarRange * 0.8 + _random.NextDouble() * radarRange * 0.2;

            var targetTypeId = targetTypes.Any() ? targetTypes[_random.Next(targetTypes.Count)].TargetTypeId : 1;
            var classificationId = classifications.Any() ? classifications[_random.Next(classifications.Count)].ClassificationId : 1;

            var target = new Target
            {
                TargetCode = $"TGT-{DateTime.Now:HHmmss}-{_random.Next(100, 999)}",
                TargetTypeId = targetTypeId,
                ClassificationId = classificationId,
                PositionX = Math.Cos(angle) * distance,
                PositionY = Math.Sin(angle) * distance,
                Speed = _random.Next(200, 800),
                Altitude = _random.Next(1000, 40000),
                Heading = _random.Next(0, 360),
                DetectedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                IsActive = 1,
                DetectedBy = userId
            };

            _context.Targets.Add(target);
            await _context.SaveChangesAsync();

            var targetTypeRecord = targetTypes.FirstOrDefault(t => t.TargetTypeId == targetTypeId);
            var classificationRecord = classifications.FirstOrDefault(c => c.ClassificationId == classificationId);

            await _logService.LogEventAsync(
                "Target Detected",
                $"New target detected: {target.TargetCode} ({targetTypeRecord?.Name})",
                classificationRecord?.Name == "Hostile" ? "Warning" : "Info",
                userId,
                (int)target.TargetId
            );

            if (classificationRecord?.Name == "Hostile")
            {
                var alertLevel = await _context.AlertLevels.FirstOrDefaultAsync(a => a.Name == "High");

                var alert = new ThreatAlert
                {
                    TargetId = (int)target.TargetId,
                    AlertLevelId = alertLevel?.AlertLevelId ?? 1,
                    Message = $"Hostile target detected: {target.TargetCode}",
                    IsAcknowledged = 0,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                _context.ThreatAlerts.Add(alert);
                await _context.SaveChangesAsync();
            }

            return target;
        }

        public async Task<Target?> GetTargetByIdAsync(int targetId)
        {
            return await _context.Targets.FirstOrDefaultAsync(t => t.TargetId == targetId);
        }

        public async Task UpdateTargetPositionAsync(int targetId, double posX, double posY, double heading)
        {
            var target = await _context.Targets.FirstOrDefaultAsync(t => t.TargetId == targetId);
            if (target != null)
            {
                target.PositionX = posX;
                target.PositionY = posY;
                target.Heading = heading;
                target.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ClassifyTargetAsync(int targetId, string classification, int userId)
        {
            var target = await _context.Targets
                .Include(t => t.ClassificationNavigation)
                .FirstOrDefaultAsync(t => t.TargetId == targetId);
            if (target == null)
                return false;

            var newClass = await _context.Classifications.FirstOrDefaultAsync(c => c.Name == classification);
            if (newClass == null) return false;

            var oldClassification = target.ClassificationNavigation?.Name;
            target.ClassificationId = newClass.ClassificationId;
            target.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();

            await _logService.LogEventAsync(
                "Classification Changed",
                $"Target {target.TargetCode} reclassified from {oldClassification} to {classification}",
                classification == "Hostile" ? "Warning" : "Info",
                userId,
                targetId
            );

            if (classification == "Hostile" && oldClassification != "Hostile")
            {
                var alertLevel = await _context.AlertLevels.FirstOrDefaultAsync(a => a.Name == "High");

                var alert = new ThreatAlert
                {
                    TargetId = targetId,
                    AlertLevelId = alertLevel?.AlertLevelId ?? 1,
                    Message = $"Target {target.TargetCode} classified as HOSTILE",
                    IsAcknowledged = 0,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                _context.ThreatAlerts.Add(alert);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<RadarConfiguration?> GetActiveConfigurationAsync()
        {
            return await _context.RadarConfigurations
                .FirstOrDefaultAsync(c => c.IsActive == 1);
        }

        public async Task<bool> UpdateConfigurationAsync(RadarConfiguration config)
        {
            var existing = await _context.RadarConfigurations.FirstOrDefaultAsync(c => c.ConfigId == config.ConfigId);
            if (existing == null)
                return false;

            existing.RadarRange = config.RadarRange;
            existing.ScanInterval = config.ScanInterval;
            existing.MaxTargets = config.MaxTargets;
            existing.AutoClassification = config.AutoClassification;
            existing.AlertThreshold = config.AlertThreshold;
            //existing.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();
            return true;
        }

        public List<TargetDto> SimulateTargetMovement(List<Target> targets)
        {
            var result = new List<TargetDto>();

            foreach (var target in targets)
            {
                var headingRad = target.Heading * Math.PI / 180;
                var moveDistance = target.Speed / 3600;

                var newX = target.PositionX + Math.Cos(headingRad) * moveDistance;
                var newY = target.PositionY + Math.Sin(headingRad) * moveDistance;

                result.Add(new TargetDto
                {
                    TargetId = (int)target.TargetId,
                    TargetCode = target.TargetCode,
                    TargetType = target.TargetTypeNavigation?.Name,
                    Classification = target.ClassificationNavigation?.Name,
                    PositionX = newX,
                    PositionY = newY,
                    Speed = target.Speed,
                    Altitude = target.Altitude,
                    Heading = target.Heading
                });
            }

            return result;
        }
    }
}