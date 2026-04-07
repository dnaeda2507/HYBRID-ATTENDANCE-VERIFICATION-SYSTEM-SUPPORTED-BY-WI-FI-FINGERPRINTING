using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Services
{
    public class FastApiService : IFastApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FastApiService> _logger;
        private readonly string _baseUrl;
        private readonly string _internalToken;

        public FastApiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<FastApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["FastApi:BaseUrl"] ?? "http://localhost:8000";
            _internalToken = configuration["FastApi:InternalToken"] ?? "wifi-ml-internal-secret-2024";
        }

        public async Task<FastApiPredictResult> PredictLocationAsync(List<AccessPointDto> accessPoints)
        {
            try
            {
                var payload = new
                {
                    access_points = accessPoints.ConvertAll(ap => new
                    {
                        bssid = ap.Bssid,
                        rssi = ap.Rssi
                    })
                };

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/predict")
                {
                    Content = JsonContent.Create(payload)
                };
                request.Headers.Add("X-Internal-Token", _internalToken);

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FastApiPredictResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new FastApiPredictResult { Matched = false, Message = "Boş yanıt" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastAPI tahmin hatası");
                return new FastApiPredictResult { Matched = false, Message = $"FastAPI hatası: {ex.Message}" };
            }
        }
    }
}
