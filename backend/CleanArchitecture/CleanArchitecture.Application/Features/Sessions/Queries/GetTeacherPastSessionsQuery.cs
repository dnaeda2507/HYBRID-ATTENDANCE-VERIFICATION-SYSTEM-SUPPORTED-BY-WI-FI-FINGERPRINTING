using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Sessions;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Sessions.Queries
{
    public class GetTeacherPastSessionsQuery : IRequest<List<TeacherSessionDTO>>
    {
    }

    public class GetTeacherPastSessionsQueryHandler : IRequestHandler<GetTeacherPastSessionsQuery, List<TeacherSessionDTO>>
    {
        private readonly ISessionRepositoryAsync _sessionRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public GetTeacherPastSessionsQueryHandler(ISessionRepositoryAsync sessionRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _sessionRepository = sessionRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<List<TeacherSessionDTO>> Handle(GetTeacherPastSessionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;

            if (string.IsNullOrEmpty(userId))
                return new List<TeacherSessionDTO>();

            var sessions = await _sessionRepository.GetQueryableAsync()
                .Include(s => s.Course)
                    .ThenInclude(c => c.Lecture)
                .Include(s => s.Attendances)
                .Where(s => s.CreatedBy == userId)
                .OrderByDescending(s => s.Created)
                .Select(s => new TeacherSessionDTO
                {
                    Id = s.Id,
                    CourseId = s.CourseId,
                    CourseName = s.Course != null && s.Course.Lecture != null ? s.Course.Lecture.Name : "Unknown Course",
                    Date = s.Date.ToString("yyyy-MM-dd"),
                    StartTime = s.StartTime.ToString("HH:mm"),
                    EndTime = s.EndTime.ToString("HH:mm"),
                    Status = s.Status.ToString(),
                    AttendedStudentCount = s.Attendances.Count
                })
                .ToListAsync(cancellationToken);

            return sessions;
        }
    }
}
