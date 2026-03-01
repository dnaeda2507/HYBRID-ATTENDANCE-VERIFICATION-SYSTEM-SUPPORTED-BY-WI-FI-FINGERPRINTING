using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Wifi;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IWifiTrainingSampleService
    {
        Task<int> CreateAsync(StudentWifiScanCreateDto dto);
    }
}
