using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Attendances;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Attendances;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Attendances.Queries
{
    public class GetAllAttendancesQuery : IRequest<PagedResponse<AttendanceListingDTO>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Expression<Func<Attendance, bool>> Predicate { get; set; }
    }

    public class GetAllAttendancesQueryHandler : IRequestHandler<GetAllAttendancesQuery, PagedResponse<AttendanceListingDTO>>
    {
        private readonly IAttendanceRepositoryAsync _attendanceRepository;
        private readonly IMapper _mapper;

        public GetAllAttendancesQueryHandler(IAttendanceRepositoryAsync attendanceRepository, IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<AttendanceListingDTO>> Handle(GetAllAttendancesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _attendanceRepository
                    .GetQueryableAsync()
                    .Include(a => a.Student)
                    .Include(a => a.Session)
                        .ThenInclude(s => s.Course)
                            .ThenInclude(c => c.Lecture)
                    .AsQueryable();

                if (request.Predicate != null)
                {
                    query = query.Where(request.Predicate);
                }

                var totalRecords = await query.CountAsync(cancellationToken);
                var attendances = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                if (attendances == null || !attendances.Any())
                {
                    return new PagedResponse<AttendanceListingDTO>(new List<AttendanceListingDTO>(), request.PageNumber, request.PageSize, totalRecords);
                }

                var attendanceDTOs = _mapper.Map<List<AttendanceListingDTO>>(attendances);

                return new PagedResponse<AttendanceListingDTO>(attendanceDTOs, request.PageNumber, request.PageSize, totalRecords);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error retrieving {nameof(Attendance)}", ex.Message);
            }
        }
    }
}