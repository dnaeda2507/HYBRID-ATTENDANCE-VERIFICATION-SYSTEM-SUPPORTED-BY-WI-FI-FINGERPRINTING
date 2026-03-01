using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Wifi;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IStudentWifiScanService
    {
        Task<int> CreateAsync(StudentWifiScanCreateDto dto);
        Task<StudentWifiScanDto> GetByIdAsync(int id);
    }
}
