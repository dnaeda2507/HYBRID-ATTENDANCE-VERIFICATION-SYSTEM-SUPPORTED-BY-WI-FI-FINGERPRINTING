using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    /// <summary>
    /// Yoklama yönetimi ve risk analizi
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IRiskScoreService _riskScoreService;
        private readonly ISecurityEventService _securityEventService;

        public AttendanceController(IRiskScoreService riskScoreService, ISecurityEventService securityEventService)
        {
            _riskScoreService = riskScoreService;
            _securityEventService = securityEventService;
        }

        /// <summary>
        /// Tüm yoklamaları getir (placeholder - mevcut sisteme entegre edilecek)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(new { success = true, message = "Mevcut Attendance sistemi kullanınız" });
        }

        /// <summary>
        /// Belirtilen yoklamaları getir (placeholder - mevcut sisteme entegre edilecek)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(new { success = true, message = "Mevcut Attendance sistemi kullanınız" });
        }

        /// <summary>
        /// Yoklama almak için risk kontrolü yap
        /// İhtiyaç duyulursa risk skorunu güncelle
        /// </summary>
        [HttpPost("verify-attendance/{studentId}/{sessionId}")]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<IActionResult> VerifyAttendanceWithRiskCheck(string studentId, int sessionId, [FromBody] AttendanceVerificationDto dto)
        {
            try
            {
                // Risk skorunu hesapla
                var riskScore = await _riskScoreService.CalculateRiskScoreAsync(studentId);

                // Risk seviyesini belirle
                string riskLevel = riskScore.OverallRiskScore switch
                {
                    < 0.25 => "Low",
                    < 0.50 => "Medium",
                    < 0.75 => "High",
                    _ => "Critical"
                };

                // Eğer critical seviyedeyse yoklama almayı reddet
                if (riskScore.OverallRiskScore >= 0.75)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Öğrenci kritik risk seviyesindedir. Yoklama alınamaz.",
                        riskLevel = riskLevel,
                        riskScore = riskScore.OverallRiskScore,
                        action = "Admin tarafından inceleme gereklidir"
                    });
                }

                // Düşük WiFi skorunu kaydet (varsa)
                if (dto.WifiSecurityScore.HasValue && dto.WifiSecurityScore < 0.5)
                {
                    await _securityEventService.LogSecurityEventAsync(
                        studentId: studentId,
                        eventType: "low_wifi_score",
                        severity: riskScore.OverallRiskScore >= 0.6 ? "high" : "medium",
                        wifiScore: dto.WifiSecurityScore,
                        description: $"Düşük WiFi güvenlik skoru ile yoklama: {dto.WifiSecurityScore:P0}"
                    );
                }

                // IP doğrulama başarısızsa
                if (!dto.IsIpValid)
                {
                    await _securityEventService.LogSecurityEventAsync(
                        studentId: studentId,
                        eventType: "invalid_ip",
                        severity: "high",
                        description: $"Okulun ağı dışındaki IP: {dto.ClientIp}",
                        ipInvolved: dto.ClientIp
                    );
                }

                // Yoklama kaydını oluştur
                // Bu, geçerli Attendance Commands ile entegre edilecek

                return Ok(new
                {
                    success = true,
                    message = "Yoklama başarıyla kaydedildi",
                    riskLevel = riskLevel,
                    riskScore = riskScore.OverallRiskScore,
                    warningMessage = riskScore.OverallRiskScore >= 0.6 ? 
                        "⚠️ Dikkat: Öğrenci yüksek risk altında" : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Öğrencinin yoklama geçmişine bağlı olarak risk tahmini
        /// </summary>
        [HttpGet("attendance-risk-history/{studentId}")]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<IActionResult> GetAttendanceRiskHistory(string studentId, [FromQuery] int days = 30)
        {
            try
            {
                var riskScore = await _riskScoreService.GetRiskScoreAsync(studentId);
                var recentEvents = await _securityEventService.GetRecentSecurityEventsAsync(studentId, days);

                // Statistikler
                var lowWifiEvents = recentEvents.Count(e => e.EventType.Contains("low_wifi"));
                var invalidIpEvents = recentEvents.Count(e => e.EventType.Contains("invalid_ip"));
                var criticalEvents = recentEvents.Count(e => e.Severity == "Critical");

                return Ok(new
                {
                    success = true,
                    studentId = studentId,
                    riskScore = new
                    {
                        overall = riskScore.OverallRiskScore,
                        wifi = riskScore.WifiSecurityScore,
                        ip = riskScore.IpSecurityScore,
                        suspiciousEventCount = riskScore.SuspiciousEventCount,
                        isUnderReview = riskScore.IsUnderReview
                    },
                    statistics = new
                    {
                        totalEvents = recentEvents.Count,
                        lowWifiEvents = lowWifiEvents,
                        invalidIpEvents = invalidIpEvents,
                        criticalEvents = criticalEvents,
                        daysAnalyzed = days
                    },
                    recentEvents = recentEvents.Select(e => new
                    {
                        id = e.Id,
                        eventType = e.EventType,
                        severity = e.Severity,
                        description = e.Description,
                        detectedAt = e.DetectedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Öğrencinin çelişkili yoklamaları kontrol et
        /// (Aynı anda farklı sınıflarda yoklama vb.)
        /// </summary>
        [HttpGet("check-anomalies/{studentId}")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<IActionResult> CheckAttendanceAnomalies(string studentId)
        {
            try
            {
                var recentEvents = await _securityEventService.GetRecentSecurityEventsAsync(studentId, 1);
                var anomalies = new List<string>();

                // Anomali 1: Aynı gün içinde çok farklı BSSID'lerden yoklama
                if (recentEvents.Count > 1)
                {
                    var bssidSet = recentEvents.Where(e => !string.IsNullOrEmpty(e.BSSIDInvolved))
                        .Select(e => e.BSSIDInvolved).Distinct().ToList();
                    
                    if (bssidSet.Count > 2)
                    {
                        anomalies.Add($"Birden fazla WiFi erişim noktasından yoklama: {string.Join(", ", bssidSet)}");
                    }
                }

                // Anomali 2: Düşük WiFi skorlarının sık tekrar etmesi
                var lowWifiCount = recentEvents.Count(e => e.EventType.Contains("low_wifi"));
                if (lowWifiCount > 3)
                {
                    anomalies.Add($"Sık düşük WiFi skorları ({lowWifiCount} olay)");
                }

                // Anomali 3: Çeşitli IP adreslerinden erişim
                var ipSet = recentEvents.Where(e => !string.IsNullOrEmpty(e.IpInvolved))
                    .Select(e => e.IpInvolved).Distinct().ToList();
                if (ipSet.Count > 3)
                {
                    anomalies.Add($"Birden fazla IP adresinden erişim: {string.Join(", ", ipSet)}");
                }

                return Ok(new
                {
                    success = true,
                    studentId = studentId,
                    hasAnomalies = anomalies.Count > 0,
                    anomalies = anomalies
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Yoklama doğrulama DTO'su
    /// </summary>
    public class AttendanceVerificationDto
    {
        public double? WifiSecurityScore { get; set; }
        public bool IsIpValid { get; set; }
        public string ClientIp { get; set; }
        public string BSSIDUsed { get; set; }
        public double? ConfidenceScore { get; set; }
    }
}
