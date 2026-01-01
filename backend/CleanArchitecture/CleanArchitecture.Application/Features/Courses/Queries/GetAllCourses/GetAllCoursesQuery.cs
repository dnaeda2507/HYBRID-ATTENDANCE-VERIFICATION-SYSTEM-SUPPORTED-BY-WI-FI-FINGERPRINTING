using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Courses;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Courses;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Courses.Queries.GetAllCourses
{
    public class GetAllCoursesQuery : IRequest<PagedResponse<CourseListingDTO>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Expression<Func<Course, bool>> Predicate { get; set; }
    }

    public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, PagedResponse<CourseListingDTO>>
    {
        private readonly ICourseRepositoryAsync _courseRepository;
        private readonly IMapper _mapper;

        public GetAllCoursesQueryHandler(ICourseRepositoryAsync courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<CourseListingDTO>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            var queryable = _courseRepository.GetQueryableAsync()
                .Include(c => c.Lecture)
                .Include(c => c.Teacher)
                .Include(c => c.Departmant)
                .AsQueryable();

            if (request.Predicate != null)
            {
                queryable = queryable.Where(request.Predicate);
            }
            var totalCount = await queryable.CountAsync(cancellationToken);

            var courses = await queryable
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var courseListingDTOs = _mapper.Map<List<CourseListingDTO>>(courses);

            return new PagedResponse<CourseListingDTO>(courseListingDTOs, request.PageNumber, request.PageSize, totalCount);
        }
    }

}