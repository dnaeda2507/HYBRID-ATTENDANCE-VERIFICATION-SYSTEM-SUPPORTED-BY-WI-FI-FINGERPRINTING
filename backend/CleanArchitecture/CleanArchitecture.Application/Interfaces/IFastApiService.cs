using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Wifi;

namespace CleanArchitecture.Application.Interfaces
{
    public class FastApiPredictResult
    {
        public bool Matched { get; set; }
        public string ClassroomName { get; set; }
        public string ClassroomId { get; set; }
        public double Confidence { get; set; }
        public string Message { get; set; }
        public bool IsSuspicious { get; set; }
    }

    public interface IFastApiService
    {
        Task<FastApiPredictResult> PredictLocationAsync(List<AccessPointDto> accessPoints);
    }
}
