// Infrastructure/Services/StudentWifiScanService.cs
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Features.Attendances.Commands;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Core.Interfaces;
using MediatR;
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

        public StudentWifiScanService(
            IStudentWifiScanRepositoryAsync repo,
            IMapper mapper,
            IMediator mediator,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<StudentWifiScanService> logger,
            IAuthenticatedUserService currentUser)
        {
            _repo = repo;
            _mapper = mapper;
            _mediator = mediator;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
            _currentUser = currentUser;
        }

        public async Task<int> CreateAsync(StudentWifiScanCreateDto dto)
        {
            // 1. Scan'i DB'ye kaydet
            var entity = _mapper.Map<StudentWifiScan>(dto);
            var created = await _repo.AddAsync(entity);

            // 2. FastAPI'ye tahmin isteği at
            try
            {
                var prediction = await PredictAsync(dto);

                // 3. Eğer derslikte görüldüyse yoklamayı kaydet
                if (prediction?.Matched == true && dto.SessionId > 0)
                {
                    _logger.LogInformation(
                        "WiFi tahmin başarılı: {Classroom} ({Confidence:P0}). Yoklama kaydediliyor. Student={StudentId}, Session={SessionId}",
                        prediction.ClassroomName, prediction.Confidence, _currentUser.UserId, dto.SessionId);

                    await _mediator.Send(new MarkAttendanceByWifiCommand
                    {
                        SessionId = dto.SessionId,
                    });
                }
                else
                {
                    _logger.LogInformation(
                        "WiFi tahmin başarısız veya eşleşmedi. Confidence={Confidence}", prediction?.Confidence);
                }
            }
            catch (Exception ex)
            {
                // Tahmin veya yoklama hatası scan kaydını engellemez
                _logger.LogError(ex, "WiFi tahmin/yoklama hatası");
            }

            return created.Id;
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
                })
            };
            request.Headers.Add("X-Internal-Token", internalToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PredictResponse>();
        }

        private record PredictResponse(
            bool Matched,
            string? ClassroomName,
            string? ClassroomId,
            double Confidence,
            string Message);
    }
}