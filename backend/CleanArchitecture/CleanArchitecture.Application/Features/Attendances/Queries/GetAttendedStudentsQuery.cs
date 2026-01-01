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
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Attendances.Queries
{
    public class GetAttendedStudentsQuery : IRequest<List<AttendedUserDTO>>
    {
        public Expression<Func<Attendance, bool>> Predicate { get; set; }
    }

    public class GetAttendedStudentsQueryHandler : IRequestHandler<GetAttendedStudentsQuery, List<AttendedUserDTO>>
    {
        private readonly IAttendanceRepositoryAsync _attendanceRepository;
        private readonly IMapper _mapper;

        public GetAttendedStudentsQueryHandler(IAttendanceRepositoryAsync attendanceRepository, IMapper mapper)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public async Task<List<AttendedUserDTO>> Handle(GetAttendedStudentsQuery request, CancellationToken cancellationToken)
        {
            var query = _attendanceRepository
                .GetQueryableAsync($"{nameof(Attendance.Student)},{nameof(Attendance.Session)}");

            if (request.Predicate != null)
            {
                query = query.Where(request.Predicate);
            }

            var attendances = await query.ToListAsync(cancellationToken);

            if (attendances == null || !attendances.Any())
            {
                return new List<AttendedUserDTO>();
            }

            return _mapper.Map<List<AttendedUserDTO>>(attendances);
        }
    }

}