using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Lectures;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using MediatR;

namespace CleanArchitecture.Application.Features.Lectures.Queries.GetAllLectures
{
    public class GetAllLecturesQuery : IRequest<PagedResponse<LectureListingDTO>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllLecturesQueryHandler : IRequestHandler<GetAllLecturesQuery, PagedResponse<LectureListingDTO>>
    {
        private readonly ILectureRepositoryAsync _lectureRepository;
        private readonly IMapper _mapper;

        public GetAllLecturesQueryHandler(ILectureRepositoryAsync lectureRepository, IMapper mapper)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<LectureListingDTO>> Handle(GetAllLecturesQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await _lectureRepository.CountAsync();
            var lectures = await _lectureRepository.GetPagedReponseAsync(request.PageNumber, request.PageSize);
            var lectureListingDTOs = _mapper.Map<List<LectureListingDTO>>(lectures);

            return new PagedResponse<LectureListingDTO>(lectureListingDTOs, request.PageNumber, request.PageSize, totalCount);
        }
    }
}