using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Security
{
    /// <summary>
    /// Güvenlik olayı DTO'su
    /// </summary>
    public class SecurityEventDto
    {
        public int Id { get; set; }
        public string EventType { get; set; }
        public string Severity { get; set; }
        public string StudentId { get; set; }
        public string Description { get; set; }
        public string BSSIDInvolved { get; set; }
        public string IpInvolved { get; set; }
        public double? WifiSecurityScore { get; set; }
        public bool IsResolved { get; set; }
        public string ResolutionNotes { get; set; }
        public DateTime DetectedAt { get; set; }
    }

    /// <summary>
    /// Risk skoru DTO'su
    /// </summary>
    public class StudentRiskScoreDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public double OverallRiskScore { get; set; }
        public double WifiSecurityScore { get; set; }
        public double IpSecurityScore { get; set; }
        public int SuspiciousEventCount { get; set; }
        public bool IsUnderReview { get; set; }
        public DateTime? ReviewStartedAt { get; set; }
        public string ReviewReason { get; set; }
        public string ReviewNotes { get; set; }
        public double? LastAttendanceWifiScore { get; set; }
        public DateTime? LastAttendanceTime { get; set; }
        public int LowSecurityAttendanceCount { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    /// <summary>
    /// Güvenlik olayı oluştur DTO'su
    /// </summary>
    public class CreateSecurityEventDto
    {
        public string EventType { get; set; }
        public string Severity { get; set; }
        public string StudentId { get; set; }
        public string Description { get; set; }
        public string BSSIDInvolved { get; set; }
        public string IpInvolved { get; set; }
        public double? WifiSecurityScore { get; set; }
    }

    /// <summary>
    /// Risk çözümlemesi DTO'su
    /// </summary>
    public class RiskAnalysisDto
    {
        public string StudentId { get; set; }
        public double OverallRiskScore { get; set; }
        public string RiskLevel { get; set; } // "Low", "Medium", "High", "Critical"
        public List<SecurityEventDto> RecentEvents { get; set; }
        public int SuspiciousEventCount { get; set; }
        public string Recommendation { get; set; }
    }
}
