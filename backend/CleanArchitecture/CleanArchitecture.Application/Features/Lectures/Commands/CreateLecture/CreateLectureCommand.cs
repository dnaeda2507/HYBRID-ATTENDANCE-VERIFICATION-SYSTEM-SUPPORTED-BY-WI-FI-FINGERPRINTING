using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using MediatR;

namespace CleanArchitecture.Application.Features.Lectures.Commands.CreateLecture
{
    public partial class CreateLectureCommand : IRequest<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateLectureCommandHandler : IRequestHandler<CreateLectureCommand, int>
    {
        private readonly ILectureRepositoryAsync _lectureRepository;
        private readonly IMapper _mapper;
        public CreateLectureCommandHandler(ILectureRepositoryAsync lectureRepository, IMapper mapper)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }
        public async Task<int> Handle(CreateLectureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (await _lectureRepository.IsCodeExists(request.Code))
                {
                    throw new ArgumentException("Lecture code already exists");
                }
                var lecture = _mapper.Map<Lecture>(request);
                await _lectureRepository.AddAsync(lecture);
                return lecture.Id;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error creating lecture", ex.Message);
            }
        }

    }
}