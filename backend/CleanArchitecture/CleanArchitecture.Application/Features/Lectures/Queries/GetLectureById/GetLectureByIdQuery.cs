using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Lectures;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using MediatR;

namespace CleanArchitecture.Application.Features.Lectures.Queries.GetLectureById
{
    public class GetLectureByIdQuery : IRequest<LectureDTO>
    {
        public int Id { get; set; }
    }

    public class GetLectureByIdQueryHandler : IRequestHandler<GetLectureByIdQuery, LectureDTO>
    {
        private readonly ILectureRepositoryAsync _lectureRepository;
        private readonly IMapper _mapper;

        public GetLectureByIdQueryHandler(ILectureRepositoryAsync lectureRepository, IMapper mapper)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }

        public async Task<LectureDTO> Handle(GetLectureByIdQuery request, CancellationToken cancellationToken)
        {
            var lecture = await _lectureRepository.GetByIdAsync(request.Id);
            if (lecture == null)
            {
                throw new ArgumentException($"{nameof(Lecture)} not found");
            }

            return _mapper.Map<LectureDTO>(lecture);
        }
    }
}