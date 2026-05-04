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
    /// Güvenlik olaylarını işleyen ve otomatik inceleme mekanizmasını yöneten servis
    /// </summary>
    public interface ISecurityEventService
    {
        Task<SecurityEvent> LogSecurityEventAsync(string studentId, string eventType, string severity, 
            double? wifiScore = null, string description = null, string bssidInvolved = null, string ipInvolved = null);
        
        Task<List<SecurityEvent>> GetRecentSecurityEventsAsync(string studentId, int daysBack = 30);
        
        Task<bool> ShouldMarkForReviewAsync(string studentId);
        
        Task ReviewStudentAsync(string studentId, string reason, string notes = null);
        
        Task ClearReviewStatusAsync(string studentId, string notes);
        
        Task<int> GetUnresolvedEventCountAsync(string studentId);
        
        Task ResolveEventAsync(int eventId, string resolutionNotes);
        
        Task<List<SecurityEvent>> GetUnresolvedEventsAsync(string studentId);
    }

    public class SecurityEventService : ISecurityEventService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IRiskScoreService _riskScoreService;

        public SecurityEventService(ApplicationDbContext dbContext, IRiskScoreService riskScoreService)
        {
            _dbContext = dbContext;
            _riskScoreService = riskScoreService;
        }

        /// <summary>
        /// Güvenlik olayı kaydet
        /// </summary>
        public async Task<SecurityEvent> LogSecurityEventAsync(string studentId, string eventType, string severity,
            double? wifiScore = null, string description = null, string bssidInvolved = null, string ipInvolved = null)
        {
            var securityEvent = new SecurityEvent
            {
                StudentId = studentId,
                EventType = eventType,
                Severity = severity,
                Description = description,
                BSSIDInvolved = bssidInvolved,
                IpInvolved = ipInvolved,
                WifiSecurityScore = wifiScore,
                IsResolved = false,
                DetectedAt = DateTime.UtcNow
            };

            _dbContext.SecurityEvents.Add(securityEvent);
            await _dbContext.SaveChangesAsync();

            // Risk skorunu güncelle
            await _riskScoreService.UpdateRiskScoreAsync(studentId);

            // Otomatik inceleme kontrolü yap
            await CheckAndMarkForReviewAsync(studentId);

            return securityEvent;
        }

        /// <summary>
        /// Son X günün güvenlik olaylarını getir
        /// </summary>
        public async Task<List<SecurityEvent>> GetRecentSecurityEventsAsync(string studentId, int daysBack = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-daysBack);

            return await _dbContext.SecurityEvents
                .Where(se => se.StudentId == studentId && se.DetectedAt >= startDate)
                .OrderByDescending(se => se.DetectedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Otomatik inceleme kontrolü yap ve işaretleme yap
        /// </summary>
        private async Task CheckAndMarkForReviewAsync(string studentId)
        {
            var riskScore = await _riskScoreService.GetRiskScoreAsync(studentId);

            if (riskScore.OverallRiskScore >= 0.6 && !riskScore.IsUnderReview)
            {
                // Otomatik olarak inceleme altına al
                riskScore.IsUnderReview = true;
                riskScore.ReviewStartedAt = DateTime.UtcNow;
                riskScore.ReviewReason = $"Otomatik sistem: Risk skoru {riskScore.OverallRiskScore:P0} eşiğini aştı";
                
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// İnceleme için işaretlenecek mi kontrolü
        /// </summary>
        public async Task<bool> ShouldMarkForReviewAsync(string studentId)
        {
            var riskScore = await _riskScoreService.GetRiskScoreAsync(studentId);
            return riskScore.OverallRiskScore >= 0.6 && !riskScore.IsUnderReview;
        }

        /// <summary>
        /// Öğrenciyi inceleme altına al
        /// </summary>
        public async Task ReviewStudentAsync(string studentId, string reason, string notes = null)
        {
            var riskScore = await _dbContext.StudentRiskScores
                .FirstOrDefaultAsync(rs => rs.StudentId == studentId);

            if (riskScore == null)
            {
                riskScore = new StudentRiskScore { StudentId = studentId };
                _dbContext.StudentRiskScores.Add(riskScore);
            }

            riskScore.IsUnderReview = true;
            riskScore.ReviewStartedAt = DateTime.UtcNow;
            riskScore.ReviewReason = reason;
            riskScore.ReviewNotes = notes;

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// İnceleme durumunu kaldır
        /// </summary>
        public async Task ClearReviewStatusAsync(string studentId, string notes)
        {
            var riskScore = await _dbContext.StudentRiskScores
                .FirstOrDefaultAsync(rs => rs.StudentId == studentId);

            if (riskScore != null)
            {
                riskScore.IsUnderReview = false;
                riskScore.ReviewNotes = notes;
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Çözülmemiş olayların sayısı
        /// </summary>
        public async Task<int> GetUnresolvedEventCountAsync(string studentId)
        {
            return await _dbContext.SecurityEvents
                .CountAsync(se => se.StudentId == studentId && !se.IsResolved);
        }

        /// <summary>
        /// Olayı çöz
        /// </summary>
        public async Task ResolveEventAsync(int eventId, string resolutionNotes)
        {
            var securityEvent = await _dbContext.SecurityEvents.FindAsync(eventId);
            if (securityEvent != null)
            {
                securityEvent.IsResolved = true;
                securityEvent.ResolutionNotes = resolutionNotes;
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Çözülmemiş olayları getir
        /// </summary>
        public async Task<List<SecurityEvent>> GetUnresolvedEventsAsync(string studentId)
        {
            return await _dbContext.SecurityEvents
                .Where(se => se.StudentId == studentId && !se.IsResolved)
                .OrderByDescending(se => se.DetectedAt)
                .ToListAsync();
        }
    }
}
