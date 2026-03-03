using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Wifi;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IWifiPredictionService
    {
        /// <summary>
        /// ML agent'a access point listesi gönder, tahmin ve confidence döner
        /// </summary>
        Task<(int ClassroomId, double Confidence)> PredictAsync(List<AccessPointDto> accessPoints);
    }
}