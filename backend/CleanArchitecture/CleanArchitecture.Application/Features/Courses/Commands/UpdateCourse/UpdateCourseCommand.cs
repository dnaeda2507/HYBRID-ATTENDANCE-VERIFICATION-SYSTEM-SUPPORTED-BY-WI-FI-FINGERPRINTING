using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Courses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Courses.Commands.UpdateCourse
{
    public partial class UpdateCourseCommand : IRequest<int>
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

    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, int>
    {
        private readonly ICourseRepositoryAsync _courseRepository;
        private readonly IMapper _mapper;

        public UpdateCourseCommandHandler(ICourseRepositoryAsync courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _courseRepository
                    .GetQueryableAsync($"{nameof(Course.Lecture)},{nameof(Course.Teacher)},{nameof(Course.Departmant)},{nameof(Course.CourseStaffs)},{nameof(Course.CourseStudents)}")
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                if (course == null)
                {
                    throw new ArgumentException($"{nameof(Course)} not found");
                }

                _mapper.Map(request, course);
                await _courseRepository.UpdateAsync(course);
                return course.Id;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error updating course", ex.Message);
            }
        }
    }
}