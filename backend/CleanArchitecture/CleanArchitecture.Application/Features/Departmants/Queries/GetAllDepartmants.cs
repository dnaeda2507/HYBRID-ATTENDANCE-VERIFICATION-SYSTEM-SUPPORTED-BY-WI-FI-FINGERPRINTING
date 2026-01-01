using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Departmant;
using CleanArchitecture.Application.Interfaces.Repositories;
using MediatR;

namespace CleanArchitecture.Application.Features.Departmants.Queries
{
    public class GetAllDepartmantsQuery : IRequest<List<DepartmantLookupDTO>>
    {

    }

    public class GetAllDepartmantsQueryHandler : IRequestHandler<GetAllDepartmantsQuery, List<DepartmantLookupDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IDepartmantRepositoryAsync _departmantRepository;

        public GetAllDepartmantsQueryHandler(IMapper mapper, IDepartmantRepositoryAsync departmantRepository)
        {
            _mapper = mapper;
            _departmantRepository = departmantRepository;
        }

        public async Task<List<DepartmantLookupDTO>> Handle(GetAllDepartmantsQuery request, CancellationToken cancellationToken)
        {
            var departmants = await _departmantRepository.GetAllAsync();
            return _mapper.Map<List<DepartmantLookupDTO>>(departmants);
        }
    }
}