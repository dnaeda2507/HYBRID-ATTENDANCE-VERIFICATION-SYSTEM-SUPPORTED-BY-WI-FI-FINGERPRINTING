using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities.Security;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Services
{
    /// <summary>
    /// Öğrencilerin risk skorlarını hesaplayan ve yöneten servis
    /// </summary>
    public interface IRiskScoreService
    {
        Task<StudentRiskScore> CalculateRiskScoreAsync(string studentId);
        Task<StudentRiskScore> GetRiskScoreAsync(string studentId);
        Task UpdateRiskScoreAsync(string studentId);
        Task<List<StudentRiskScore>> GetHighRiskStudentsAsync(double thresholdScore = 0.6);
        Task<List<StudentRiskScore>> GetStudentsUnderReviewAsync();
        Task AddSecurityEventAsync(string studentId, string eventType, string severity, double? wifiScore = null, string description = null);
    }

    public class RiskScoreService : IRiskScoreService
    {
        private readonly ApplicationDbContext _dbContext;

        public RiskScoreService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Risk skorunu hesaplar ve veritabanında günceller
        /// </summary>
        public async Task<StudentRiskScore> CalculateRiskScoreAsync(string studentId)
        {
            var riskScore = await _dbContext.StudentRiskScores
                .FirstOrDefaultAsync(rs => rs.StudentId == studentId);

            if (riskScore == null)
            {
                riskScore = new StudentRiskScore
                {
                    StudentId = studentId,
                    OverallRiskScore = 0.0,
                    WifiSecurityScore = 1.0,
                    IpSecurityScore = 1.0,
                    SuspiciousEventCount = 0,
                    IsUnderReview = false
                };
                _dbContext.StudentRiskScores.Add(riskScore);
            }

            // Son 30 günde şüpheli olayları getir
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var securityEvents = await _dbContext.SecurityEvents
                .Where(se => se.StudentId == studentId && se.DetectedAt >= thirtyDaysAgo)
                .ToListAsync();

            riskScore.SuspiciousEventCount = securityEvents.Count;

            // WiFi güvenlik skorunu hesapla
            var lowWifiScoreEvents = securityEvents
                .Where(se => se.WifiSecurityScore.HasValue && se.WifiSecurityScore < 0.5)
                .ToList();

            riskScore.LowSecurityAttendanceCount = lowWifiScoreEvents.Count;

            if (lowWifiScoreEvents.Any())
            {
                // Düşük skorların ortalaması
                var avgWifiScore = lowWifiScoreEvents.Average(se => se.WifiSecurityScore ?? 0);
                riskScore.WifiSecurityScore = Math.Max(0, avgWifiScore);
            }
            else
            {
                riskScore.WifiSecurityScore = 1.0; // Sorun yok
            }

            // IP güvenlik skorunu hesapla - invalid_ip olaylarına göre
            var ipInvalidEvents = securityEvents
                .Where(se => se.EventType.Contains("invalid_ip") || se.EventType.Contains("ip"))
                .ToList();

            if (ipInvalidEvents.Any())
            {
                riskScore.IpSecurityScore = Math.Max(0, 1.0 - (ipInvalidEvents.Count * 0.1));
            }
            else
            {
                riskScore.IpSecurityScore = 1.0;
            }

            // Ciddiyet seviyelerine göre ağırlıklandırma
            double severityScore = CalculateSeverityScore(securityEvents);

            // Genel risk skoru hesapla: %40 WiFi + %30 IP + %30 Ciddiyet
            riskScore.OverallRiskScore = (riskScore.WifiSecurityScore * 0.4) +
                                        (riskScore.IpSecurityScore * 0.3) +
                                        (severityScore * 0.3);

            // 0-1 aralığına sabitle
            riskScore.OverallRiskScore = Math.Max(0, Math.Min(1.0, riskScore.OverallRiskScore));

            riskScore.LastUpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return riskScore;
        }

        /// <summary>
        /// Ciddiyet seviyelerine göre skor hesapla
        /// </summary>
        private double CalculateSeverityScore(List<SecurityEvent> events)
        {
            if (!events.Any()) return 0;

            double score = 0;
            foreach (var evt in events)
            {
                score += evt.Severity.ToLower() switch
                {
                    "low" => 0.1,
                    "medium" => 0.3,
                    "high" => 0.6,
                    "critical" => 1.0,
                    _ => 0.1
                };
            }

            return Math.Min(1.0, score / events.Count);
        }

        /// <summary>
        /// Öğrencinin risk skorunu getir
        /// </summary>
        public async Task<StudentRiskScore> GetRiskScoreAsync(string studentId)
        {
            var riskScore = await _dbContext.StudentRiskScores
                .FirstOrDefaultAsync(rs => rs.StudentId == studentId);

            return riskScore ?? new StudentRiskScore { StudentId = studentId, OverallRiskScore = 0 };
        }

        /// <summary>
        /// Risk skorunu güncelle
        /// </summary>
        public async Task UpdateRiskScoreAsync(string studentId)
        {
            await CalculateRiskScoreAsync(studentId);
        }

        /// <summary>
        /// Yüksek risk altındaki öğrencileri getir
        /// </summary>
        public async Task<List<StudentRiskScore>> GetHighRiskStudentsAsync(double thresholdScore = 0.6)
        {
            return await _dbContext.StudentRiskScores
                .Where(rs => rs.OverallRiskScore >= thresholdScore)
                .OrderByDescending(rs => rs.OverallRiskScore)
                .ToListAsync();
        }

        /// <summary>
        /// İnceleme altında olan öğrencileri getir
        /// </summary>
        public async Task<List<StudentRiskScore>> GetStudentsUnderReviewAsync()
        {
            return await _dbContext.StudentRiskScores
                .Where(rs => rs.IsUnderReview)
                .OrderByDescending(rs => rs.ReviewStartedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Güvenlik olayı ekle
        /// </summary>
        public async Task AddSecurityEventAsync(string studentId, string eventType, string severity, 
            double? wifiScore = null, string description = null)
        {
            var securityEvent = new SecurityEvent
            {
                StudentId = studentId,
                EventType = eventType,
                Severity = severity,
                Description = description,
                WifiSecurityScore = wifiScore,
                IsResolved = false,
                DetectedAt = DateTime.UtcNow
            };

            _dbContext.SecurityEvents.Add(securityEvent);
            await _dbContext.SaveChangesAsync();

            // Risk skorunu güncelle
            await UpdateRiskScoreAsync(studentId);

            // Yüksek risk altında mı kontrol et - otomatik inceleme
            var riskScore = await GetRiskScoreAsync(studentId);
            if (riskScore.OverallRiskScore >= 0.6 && !riskScore.IsUnderReview)
            {
                riskScore.IsUnderReview = true;
                riskScore.ReviewStartedAt = DateTime.UtcNow;
                riskScore.ReviewReason = $"Otomatik bayrak: {eventType} olayından dolayı risk skoru {riskScore.OverallRiskScore:P0} civarında";
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
