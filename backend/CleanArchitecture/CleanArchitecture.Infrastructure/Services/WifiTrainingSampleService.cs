using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Core.Entities.Sessions;
using CleanArchitecture.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Services
{
    public class WifiTrainingSampleService : IWifiTrainingSampleService
    {
        private readonly IWifiTrainingSampleRepositoryAsync _repo;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly ISessionRepositoryAsync _sessionRepo;

        public WifiTrainingSampleService(
            IWifiTrainingSampleRepositoryAsync repo,
            IMapper mapper,
            HttpClient httpClient,  // ML Agent HTTP client
            ISessionRepositoryAsync sessionRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _httpClient = httpClient;
            _sessionRepo = sessionRepo;
        }

        public async Task<int> CreateAsync(StudentWifiScanCreateDto dto)
        {
            if (dto == null || dto.AccessPoints == null || dto.AccessPoints.Count == 0)
                throw new ArgumentException("Access points cannot be null or empty");

            // İlgili session ve dersten gerçek ClassroomId'yi bul
            var session = await _sessionRepo
                .GetQueryableAsync(nameof(Session.Course))
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

            if (session?.Course?.ClassroomId == null)
            {
                throw new InvalidOperationException("Course does not have a ClassroomId configured for Wi‑Fi training.");
            }

            // 1️⃣ DB kaydı — training sample gerçek sınıf id'siyle etiketlenir
            var sample = new WifiTrainingSample
            {
                ClassroomId = session.Course.ClassroomId.Value,
                CollectedAtUtc = dto.ScannedAtUtc
            };

            foreach (var ap in dto.AccessPoints)
            {
                sample.AccessPoints.Add(new WifiTrainingAccessPoint
                {
                    Bssid = ap.Bssid,
                    Rssi = ap.Rssi
                });
            }

            var created = await _repo.AddAsync(sample);

            // 2️⃣ ML Agent training isteği
            try
            {
                var trainRequest = new
                {
                    ClassroomId = sample.ClassroomId,
                    AccessPoints = dto.AccessPoints
                };

                var response = await _httpClient.PostAsJsonAsync("/train", trainRequest);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Hata durumunda log at, DB kaydı yine geçerli
            }

            return created.Id;
        }
    }
}