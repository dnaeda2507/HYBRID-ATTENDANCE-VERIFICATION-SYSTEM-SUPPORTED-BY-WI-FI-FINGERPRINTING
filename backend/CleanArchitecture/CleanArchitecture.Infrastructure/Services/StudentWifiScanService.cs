using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.Infrastructure.Services
{
    public class StudentWifiScanService : IStudentWifiScanService
    {
        private readonly IStudentWifiScanRepositoryAsync _repo;
        private readonly IMapper _mapper;

        public StudentWifiScanService(IStudentWifiScanRepositoryAsync repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<int> CreateAsync(StudentWifiScanCreateDto dto)
        {
            var entity = _mapper.Map<StudentWifiScan>(dto);
            var created = await _repo.AddAsync(entity);
            return created.Id;
        }

        public async Task<StudentWifiScanDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<StudentWifiScanDto>(entity);
        }
    }
}
