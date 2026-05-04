using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Core.Entities.Security
{
    /// <summary>
    /// Her öğrenci için risk skorunu ve inceleme durumunu takip eden entity
    /// </summary>
    [Table("StudentRiskScores")]
    public class StudentRiskScore : AuditableBaseEntity
    {
        /// <summary>
        /// Öğrenci ID'si (benzersiz)
        /// </summary>
        [Required]
        public string StudentId { get; set; }

        /// <summary>
        /// Genel risk skoru (0.0 - 1.0, 1.0 = en yüksek risk)
        /// </summary>
        [Required]
        public double OverallRiskScore { get; set; } = 0.0;

        /// <summary>
        /// WiFi güvenlik skoru (0.0 - 1.0)
        /// </summary>
        public double WifiSecurityScore { get; set; } = 1.0;

        /// <summary>
        /// IP adresi güvenlik skoru (0.0 - 1.0)
        /// </summary>
        public double IpSecurityScore { get; set; } = 1.0;

        /// <summary>
        /// Son 30 günde şüpheli olayların sayısı
        /// </summary>
        public int SuspiciousEventCount { get; set; } = 0;

        /// <summary>
        /// Öğrenci inceleme altına alındı mı?
        /// </summary>
        public bool IsUnderReview { get; set; } = false;

        /// <summary>
        /// İnceleme durumunun başladığı tarih
        /// </summary>
        public DateTime? ReviewStartedAt { get; set; }

        /// <summary>
        /// İnceleme nedeni
        /// </summary>
        public string ReviewReason { get; set; }

        /// <summary>
        /// İnceleme notları
        /// </summary>
        public string ReviewNotes { get; set; }

        /// <summary>
        /// Son yoklama çeşidinde gerçekleştirilen güvenlik skoru
        /// </summary>
        public double? LastAttendanceWifiScore { get; set; }

        /// <summary>
        /// Son yoklama zamanı
        /// </summary>
        public DateTime? LastAttendanceTime { get; set; }

        /// <summary>
        /// Risk puanının son güncellenme zamanı
        /// </summary>
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Son 30 günde düşük güvenlik skoru ile yapılan yoklama sayısı
        /// </summary>
        public int LowSecurityAttendanceCount { get; set; } = 0;

        /// <summary>
        /// İlişki: Öğrenci
        /// </summary>
        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser Student { get; set; }
    }
}
