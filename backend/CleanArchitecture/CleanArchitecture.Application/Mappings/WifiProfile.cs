using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Core.Entities.Wifi;

namespace CleanArchitecture.Application.Mappings
{
    public class WifiProfile : Profile
    {
        public WifiProfile()
        {
            CreateMap<StudentWifiScanCreateDto, StudentWifiScan>();
            CreateMap<StudentWifiScan, StudentWifiScanDto>();
            CreateMap<AccessPointDto, StudentWifiAccessPoint>().ReverseMap();
            CreateMap<AccessPointDto, WifiTrainingAccessPoint>().ReverseMap();
        }
    }
}
