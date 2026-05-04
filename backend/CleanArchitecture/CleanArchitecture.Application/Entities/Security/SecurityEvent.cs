using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Core.Entities.Security
{
    /// <summary>
    /// Şüpheli aktiviteler ve güvenlik olaylarını kaydetmek için kullanılan entity
    /// </summary>
    [Table("SecurityEvents")]
    public class SecurityEvent : AuditableBaseEntity
    {
        /// <summary>
        /// Olayın türü (low_wifi_score, invalid_ip, fake_bssid, vb.)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string EventType { get; set; }

        /// <summary>
        /// Ciddiyet seviyesi (Low, Medium, High, Critical)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Severity { get; set; }

        /// <summary>
        /// İlgili öğrenci ID'si
        /// </summary>
        [Required]
        public string StudentId { get; set; }

        /// <summary>
        /// İlgili başarısız yoklama çeşidi
        /// </summary>
        public string AttendanceSessionId { get; set; }

        /// <summary>
        /// Olayın açıklaması
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// İlgili BSSID (WiFi erişim noktası)
        /// </summary>
        [StringLength(50)]
        public string BSSIDInvolved { get; set; }

        /// <summary>
        /// İlgili IP adresi
        /// </summary>
        [StringLength(50)]
        public string IpInvolved { get; set; }

        /// <summary>
        /// WiFi güvenlik skoru (0.0 - 1.0)
        /// </summary>
        public double? WifiSecurityScore { get; set; }

        /// <summary>
        /// Olayın çözülüp çözülmediği
        /// </summary>
        public bool IsResolved { get; set; } = false;

        /// <summary>
        /// Çözüm notları
        /// </summary>
        public string ResolutionNotes { get; set; }

        /// <summary>
        /// Olayın tespit edildiği tarih
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// İlişki: Öğrenci
        /// </summary>
        [ForeignKey(nameof(StudentId))]
        public virtual ApplicationUser Student { get; set; }
    }
}
