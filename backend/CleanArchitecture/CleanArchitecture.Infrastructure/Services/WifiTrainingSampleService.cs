using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.Infrastructure.Services
{
    public class WifiTrainingSampleService : IWifiTrainingSampleService
    {
        private readonly IWifiTrainingSampleRepositoryAsync _repo;
        private readonly IMapper _mapper;

        public WifiTrainingSampleService(IWifiTrainingSampleRepositoryAsync repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<int> CreateAsync(StudentWifiScanCreateDto dto)
        {
            // Minimal: reuse StudentWifiScanCreateDto shape for sample creation
            var sample = new WifiTrainingSample
            {
                ClassroomId = dto.SessionId, // placeholder mapping - adapt as needed
                CollectedAtUtc = dto.ScannedAtUtc
            };
            var created = await _repo.AddAsync(sample);
            return created.Id;
        }
    }
}
