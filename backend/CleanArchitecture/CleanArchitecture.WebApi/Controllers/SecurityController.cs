using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Security;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers
{
    /// <summary>
    /// Sistem güvenliği ve risk yönetimi için API endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly IRiskScoreService _riskScoreService;
        private readonly ISecurityEventService _securityEventService;
        private readonly IMapper _mapper;

        public SecurityController(IRiskScoreService riskScoreService, ISecurityEventService securityEventService, IMapper mapper)
        {
            _riskScoreService = riskScoreService;
            _securityEventService = securityEventService;
            _mapper = mapper;
        }

        /// <summary>
        /// Öğrencinin risk skorunu hesapla ve getir
        /// </summary>
        [HttpGet("risk-score/{studentId}")]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<ActionResult<StudentRiskScoreDto>> GetRiskScore(string studentId)
        {
            try
            {
                var riskScore = await _riskScoreService.CalculateRiskScoreAsync(studentId);
                var dto = _mapper.Map<StudentRiskScoreDto>(riskScore);
                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Yüksek riskli öğrencileri listele
        /// </summary>
        [HttpGet("high-risk-students")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<ActionResult<List<StudentRiskScoreDto>>> GetHighRiskStudents([FromQuery] double threshold = 0.6)
        {
            try
            {
                var highRiskStudents = await _riskScoreService.GetHighRiskStudentsAsync(threshold);
                var dtos = _mapper.Map<List<StudentRiskScoreDto>>(highRiskStudents);
                return Ok(new { success = true, count = dtos.Count, data = dtos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// İnceleme altında olan öğrencileri listele
        /// </summary>
        [HttpGet("students-under-review")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<ActionResult<List<StudentRiskScoreDto>>> GetStudentsUnderReview()
        {
            try
            {
                var studentsUnderReview = await _riskScoreService.GetStudentsUnderReviewAsync();
                var dtos = _mapper.Map<List<StudentRiskScoreDto>>(studentsUnderReview);
                return Ok(new { success = true, count = dtos.Count, data = dtos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Güvenlik olayı kaydet
        /// </summary>
        [HttpPost("log-event")]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<ActionResult<SecurityEventDto>> LogSecurityEvent([FromBody] CreateSecurityEventDto dto)
        {
            try
            {
                var securityEvent = await _securityEventService.LogSecurityEventAsync(
                    dto.StudentId,
                    dto.EventType,
                    dto.Severity,
                    dto.WifiSecurityScore,
                    dto.Description,
                    dto.BSSIDInvolved,
                    dto.IpInvolved
                );

                var eventDto = _mapper.Map<SecurityEventDto>(securityEvent);
                return Ok(new { success = true, message = "Olayı başarıyla kaydettik", data = eventDto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Öğrencinin son güvenlik olaylarını getir
        /// </summary>
        [HttpGet("recent-events/{studentId}")]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<ActionResult<List<SecurityEventDto>>> GetRecentSecurityEvents(string studentId, [FromQuery] int daysBack = 30)
        {
            try
            {
                var events = await _securityEventService.GetRecentSecurityEventsAsync(studentId, daysBack);
                var dtos = _mapper.Map<List<SecurityEventDto>>(events);
                return Ok(new { success = true, count = dtos.Count, data = dtos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Çözülmemiş olayları getir
        /// </summary>
        [HttpGet("unresolved-events/{studentId}")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<ActionResult<List<SecurityEventDto>>> GetUnresolvedEvents(string studentId)
        {
            try
            {
                var events = await _securityEventService.GetUnresolvedEventsAsync(studentId);
                var dtos = _mapper.Map<List<SecurityEventDto>>(events);
                return Ok(new { success = true, count = dtos.Count, data = dtos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Öğrenciyi inceleme altına al
        /// </summary>
        [HttpPost("mark-for-review")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<ActionResult> MarkStudentForReview([FromQuery] string studentId, [FromBody] ReviewRequestDto request) // ✅ [FromQuery] eklendi
        {
            try
            {
                await _securityEventService.ReviewStudentAsync(studentId, request.Reason, request.Notes);
                return Ok(new { success = true, message = "Öğrenci inceleme altına alındı" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// İncelemeyi sonlandır
        /// </summary>
        [HttpPost("clear-review")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<ActionResult> ClearReviewStatus([FromQuery] string studentId, [FromBody] ClearReviewRequestDto request) // ✅ [FromQuery] eklendi
        {
            try
            {
                await _securityEventService.ClearReviewStatusAsync(studentId, request.Notes);
                return Ok(new { success = true, message = "İnceleme durumu temizlendi" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Güvenlik olayını çöz
        /// </summary>
        [HttpPost("resolve-event/{eventId}")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<ActionResult> ResolveEvent(int eventId, [FromBody] ResolveEventRequestDto request)
        {
            try
            {
                await _securityEventService.ResolveEventAsync(eventId, request.ResolutionNotes);
                return Ok(new { success = true, message = "Olayı başarıyla çözdük" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Risk analizi yap
        /// </summary>
        [HttpGet("risk-analysis/{studentId}")]
        [Authorize(Roles = "Admin,ItStaff,Teacher")]
        public async Task<ActionResult<RiskAnalysisDto>> GetRiskAnalysis(string studentId)
        {
            try
            {
                var riskScore = await _riskScoreService.CalculateRiskScoreAsync(studentId);
                var recentEvents = await _securityEventService.GetRecentSecurityEventsAsync(studentId);
                var eventDtos = _mapper.Map<List<SecurityEventDto>>(recentEvents);

                // Risk seviyesini belirle
                string riskLevel = riskScore.OverallRiskScore switch
                {
                    < 0.25 => "Low",
                    < 0.50 => "Medium",
                    < 0.75 => "High",
                    _ => "Critical"
                };

                // Öneriye karar ver
                string recommendation = riskScore.OverallRiskScore switch
                {
                    < 0.25 => "Öğrenci güvenli bir şekilde yoklama alabilir.",
                    < 0.50 => "Öğrenci statüsü dikkatle izlenmelidir.",
                    < 0.75 => "Öğrencinin kimlik doğrulaması incelenmelidir.",
                    _ => "Öğrenci için acil inceleme gereklidir. Yoklama alımı durdurulmalıdır."
                };

                var analysis = new RiskAnalysisDto
                {
                    StudentId = studentId,
                    OverallRiskScore = riskScore.OverallRiskScore,
                    RiskLevel = riskLevel,
                    RecentEvents = eventDtos,
                    SuspiciousEventCount = riskScore.SuspiciousEventCount,
                    Recommendation = recommendation
                };

                return Ok(new { success = true, data = analysis });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Sistem istatistikleri
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,ItStaff")]
        public async Task<ActionResult> GetSecurityStatistics()
        {
            try
            {
                var highRiskStudents = await _riskScoreService.GetHighRiskStudentsAsync(0.6);
                var studentsUnderReview = await _riskScoreService.GetStudentsUnderReviewAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        highRiskStudentsCount = highRiskStudents.Count,
                        studentsUnderReviewCount = studentsUnderReview.Count,
                        averageRiskScore = highRiskStudents.Any() ? highRiskStudents.Average(rs => rs.OverallRiskScore) : 0,
                        totalSecurityEvents = 0 // Tüm olayların sayısı
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // Helper DTOs
    public class ReviewRequestDto
    {
        public string Reason { get; set; }
        public string Notes { get; set; }
    }

    public class ClearReviewRequestDto
    {
        public string Notes { get; set; }
    }

    public class ResolveEventRequestDto
    {
        public string ResolutionNotes { get; set; }
    }
}
