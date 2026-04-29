using System;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Security;
using CleanArchitecture.Core.Entities.Security;

namespace CleanArchitecture.Core.Mappings
{
    /// <summary>
    /// Security entity'leri için AutoMapper profili
    /// </summary>
    public class SecurityMappingProfile : Profile
    {
        public SecurityMappingProfile()
        {
            // SecurityEvent mappings
            CreateMap<SecurityEvent, SecurityEventDto>().ReverseMap();
            
            // StudentRiskScore mappings
            CreateMap<StudentRiskScore, StudentRiskScoreDto>().ReverseMap();
            
            // CreateSecurityEventDto to SecurityEvent
            CreateMap<CreateSecurityEventDto, SecurityEvent>()
                .ForMember(dest => dest.DetectedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
