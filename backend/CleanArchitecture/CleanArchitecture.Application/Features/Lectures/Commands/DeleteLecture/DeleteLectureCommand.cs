using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Lectures;
using MediatR;

namespace CleanArchitecture.Application.Features.Lectures.Commands.DeleteLecture
{
    public partial class DeleteLectureCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteLectureCommandHandler : IRequestHandler<DeleteLectureCommand, int>
    {
        private readonly ILectureRepositoryAsync _lectureRepository;

        public DeleteLectureCommandHandler(ILectureRepositoryAsync lectureRepository)
        {
            _lectureRepository = lectureRepository;
        }

        public async Task<int> Handle(DeleteLectureCommand request, CancellationToken cancellationToken)
        {
            var lecture = await _lectureRepository.GetByIdAsync(request.Id);
            if (lecture == null)
            {
                throw new ArgumentException($"{nameof(Lecture)} not found");
            }

            await _lectureRepository.DeleteAsync(lecture);
            return lecture.Id;
        }
    }

}