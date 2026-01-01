using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Courses;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Courses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Courses.Queries.GetCourse
{
    public class GetCourseQuery : IRequest<CourseDetailDTO>
    {
        public Expression<Func<Course, bool>> Predicate { get; set; }
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, CourseDetailDTO>
    {
        private readonly ICourseRepositoryAsync _courseRepository;
        private readonly IMapper _mapper;

        public GetCourseQueryHandler(ICourseRepositoryAsync courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<CourseDetailDTO> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _courseRepository
                    .GetQueryableAsync($"{nameof(Course.Lecture)},{nameof(Course.Teacher)},{nameof(Course.Departmant)}")
                    .Include(c => c.CourseStaffs)
                        .ThenInclude(cs => cs.Staff)
                    .Include(c => c.CourseStudents)
                        .ThenInclude(cs => cs.Student)
                    .FirstOrDefaultAsync(request.Predicate, cancellationToken);

                if (course == null)
                {
                    return null;
                }

                return _mapper.Map<CourseDetailDTO>(course);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error retrieving {nameof(Course)}", ex.Message);
            }
        }
    }

}