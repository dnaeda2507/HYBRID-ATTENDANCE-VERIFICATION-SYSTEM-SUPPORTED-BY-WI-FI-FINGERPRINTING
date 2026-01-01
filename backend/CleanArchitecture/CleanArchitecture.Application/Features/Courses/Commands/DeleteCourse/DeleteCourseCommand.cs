using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces.Repositories;
using MediatR;

namespace CleanArchitecture.Application.Features.Courses.Commands.DeleteCourse
{
    public partial class DeleteCourseCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, int>
    {
        private readonly ICourseRepositoryAsync _courseRepository;

        public DeleteCourseCommandHandler(ICourseRepositoryAsync courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<int> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdAsync(request.Id);
            if (course == null)
            {
                throw new ArgumentException("Course not found");
            }
            await _courseRepository.DeleteAsync(course);
            return course.Id;
        }
    }

}