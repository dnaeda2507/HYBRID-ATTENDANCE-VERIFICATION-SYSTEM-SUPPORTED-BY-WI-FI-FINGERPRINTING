// Infrastructure/Services/StudentWifiScanService.cs
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Features.Attendances.Commands;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Services
{
    public class StudentWifiScanService : IStudentWifiScanService
    {
        private readonly IStudentWifiScanRepositoryAsync _repo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<StudentWifiScanService> _logger;
        private readonly IAuthenticatedUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISessionRepositoryAsync _sessionRepo;

        public StudentWifiScanService(
            IStudentWifiScanRepositoryAsync repo,
            IMapper mapper,
            IMediator mediator,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<StudentWifiScanService> logger,
            IAuthenticatedUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            ISessionRepositoryAsync sessionRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _mediator = mediator;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _sessionRepo = sessionRepo;
        }

        public async Task<WifiScanResultDto> CreateAsync(StudentWifiScanCreateDto dto)
        {
            // 1. Scan'i DB'ye kaydet
            var entity = _mapper.Map<StudentWifiScan>(dto);
            var created = await _repo.AddAsync(entity);

            var result = new WifiScanResultDto
            {
                ScanId = created.Id,
                AttendanceMarked = false,
                Message = "WiFi taraması kaydedildi ancak derslikte bulunulamadı.",
                Confidence = 0
            };

            // 2. FastAPI'ye tahmin isteği at
            try
            {
                var prediction = await PredictAsync(dto);

                // 3. Eğer derslikte görüldüyse yoklamayı kaydet
                if (prediction?.Matched == true && dto.SessionId > 0)
                {
                    // Öğretmenin açtığı dersin lokasyonunu veritabanından çekiyoruz
                    var session = await _sessionRepo.GetQueryableAsync()
                        .Include(s => s.Course)
                        .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

                    if (session != null && session.Course != null)
                    {
                        // C101, C-101, c 101 gibi farklı yazımları tolere etmek için normalize ediyoruz
                        string expectedLocation = session.Course.Location?.Replace(" ", "").Replace("-", "").ToLower() ?? "";
                        string predictedLocation = prediction.ClassroomName?.Replace(" ", "").Replace("-", "").ToLower() ?? "";

                        if (!string.IsNullOrEmpty(expectedLocation) && expectedLocation == predictedLocation)
                        {
                            _logger.LogInformation(
                                "WiFi tahmin başarılı ve lokasyon EŞLEŞTİ: {PredictedClassroom} == {ExpectedClassroom} ({Confidence:P0}). Yoklama kaydediliyor. Student={StudentId}, Session={SessionId}",
                                prediction.ClassroomName, session.Course.Location, prediction.Confidence, _currentUser.UserId, dto.SessionId);

                            await _mediator.Send(new MarkAttendanceCommand
                            {
                                SessionId = dto.SessionId,
                                Method = (AttendanceMethod)2,
                                IsSuspicious = prediction.IsSuspicious
                            });

                            result.AttendanceMarked = true;
                            result.Message = $"Yoklama alındı ✓ ({prediction.ClassroomName}, %{prediction.Confidence * 100:F0} güven)";
                            result.Confidence = prediction.Confidence;
                        }
                        else
                        {
                            _logger.LogWarning(
                                "WiFi tahmin başarılı FAKAT lokasyon UYUŞMUYOR. Öğrencinin Yoklaması REDDEDİLDİ. Beklenen: {ExpectedClassroom}, Bulunan: {PredictedClassroom}",
                                session.Course.Location, prediction.ClassroomName);
                            result.Message = $"Derslikte bulunulamadı. Beklenen: {session.Course.Location}, Bulunan: {prediction.ClassroomName}";
                            result.Confidence = prediction.Confidence;
                        }
                    }
                }
                else
                {
                    var conf = prediction?.Confidence ?? 0;
                    _logger.LogInformation("WiFi tahmin başarısız veya eşleşmedi. Confidence={Confidence}", conf);
                    result.Message = $"Derslikte bulunulamadı. (güven: %{conf * 100:F0})";
                    result.Confidence = conf;
                }
            }
            catch (Exception ex)
            {
                // Tahmin veya yoklama hatası scan kaydını engellemez
                _logger.LogError(ex, "WiFi tahmin/yoklama hatası");
                result.Message = "WiFi tahmin servisi hatası: " + ex.Message;
            }

            return result;
        }

        public async Task<StudentWifiScanDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<StudentWifiScanDto>(entity);
        }

        // --- Private ---

        private async Task<PredictResponse?> PredictAsync(StudentWifiScanCreateDto dto)
        {
            var client = _httpClientFactory.CreateClient("FastAPI");
            var internalToken = _config["FastAPI:InternalToken"];
            var studentId = _currentUser.UserId;
            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

            var request = new HttpRequestMessage(HttpMethod.Post, "/predict")
            {
                Content = JsonContent.Create(new
                {
                    access_points = dto.AccessPoints?.Select(ap => new
                    {
                        bssid = ap.Bssid,
                        rssi = ap.Rssi,
                    }),
                    session_id = dto.SessionId,
                    student_id = studentId
                })
            };
            request.Headers.Add("X-Internal-Token", internalToken);
            request.Headers.Add("X-Forwarded-For", clientIp);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower  // is_suspicious → IsSuspicious
            };
            return await response.Content.ReadFromJsonAsync<PredictResponse>(jsonOptions);
        }

        private record PredictResponse(
            bool Matched,
            string? ClassroomName,
            string? ClassroomId,
            double Confidence,
            string Message,
            bool IsSuspicious);
    }
}