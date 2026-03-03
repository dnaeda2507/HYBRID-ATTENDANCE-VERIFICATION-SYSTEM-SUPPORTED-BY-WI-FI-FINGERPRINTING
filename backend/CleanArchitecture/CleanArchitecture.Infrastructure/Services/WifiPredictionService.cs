using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Interfaces;

namespace CleanArchitecture.Infrastructure.Services
{
    public class WifiPredictionService : IWifiPredictionService
    {
        private readonly HttpClient _httpClient;

        public WifiPredictionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(int ClassroomId, double Confidence)> PredictAsync(List<AccessPointDto> accessPoints)
        {
            // ML Agent endpoint: http://localhost:8000/predict
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/predict", new { accessPoints });
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PredictResponse>();
                if (result == null)
                    return (-1, 0.0);
                return (result.ClassroomId, result.Confidence);
            }
            catch
            {
                // On error, do not throw to caller; return sentinel no-prediction
                return (-1, 0.0);
            }
        }
    }

    public class PredictResponse
    {
        public int ClassroomId { get; set; }
        public double Confidence { get; set; }
    }
}