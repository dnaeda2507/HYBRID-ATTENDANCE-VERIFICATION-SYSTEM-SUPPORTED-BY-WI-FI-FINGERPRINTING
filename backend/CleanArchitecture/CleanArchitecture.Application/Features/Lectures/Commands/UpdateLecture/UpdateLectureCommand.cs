using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using MediatR;

namespace CleanArchitecture.Application.Features.Lectures.Commands.UpdateLecture
{
    public partial class UpdateLectureCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class UpdateLectureCommandHandler : IRequestHandler<UpdateLectureCommand, int>
    {
        private readonly ILectureRepositoryAsync _lectureRepository;
        private readonly IMapper _mapper;

        public UpdateLectureCommandHandler(ILectureRepositoryAsync lectureRepository, IMapper mapper)
        {
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateLectureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var lecture = await _lectureRepository.GetByIdAsync(request.Id);
                if (lecture == null)
                {
                    throw new ArgumentException($"{nameof(Lecture)} not found");
                }

                _mapper.Map(request, lecture);
                await _lectureRepository.UpdateAsync(lecture);
                return lecture.Id;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error updating {nameof(Lecture)}", ex.Message);
            }
        }
    }

}