using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Courses;
using MediatR;

namespace CleanArchitecture.Application.Features.Courses.Commands.CreateCourse
{
    public partial class CreateCourseCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int LectureId { get; set; }
        public string TeacherId { get; set; }
        public int DepartmantId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Location { get; set; }
        public List<string> CourseStaffIds { get; set; }
        public List<string> CourseStudentIds { get; set; }
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, int>
    {
        private readonly ICourseRepositoryAsync _courseRepository;
        private readonly IMapper _mapper;

        public CreateCourseCommandHandler(ICourseRepositoryAsync courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var isCourseExists = await _courseRepository.GetByIdAsync(request.Id);
                if (isCourseExists != null)
                {
                    throw new ArgumentException("Course already exists");
                }
                var course = _mapper.Map<Course>(request);
                await _courseRepository.AddAsync(course);
                return course.Id;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error creating course", ex.Message);
            }
        }
    }
}