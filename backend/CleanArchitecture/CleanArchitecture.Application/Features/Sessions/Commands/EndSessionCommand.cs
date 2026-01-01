using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Sessions;
using CleanArchitecture.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Attendances.Commands
{
    public partial class EndSessionCommand : IRequest<Unit>
    {
        public int SessionId { get; set; }
    }

    public class EndSessionCommandHandler
        : IRequestHandler<EndSessionCommand, Unit>
    {
        private readonly ISessionRepositoryAsync _sessionRepo;
        private readonly ICurrentUserService _currentUser;

        public EndSessionCommandHandler(
            ISessionRepositoryAsync sessionRepo,
            ICurrentUserService currentUser)
        {
            _sessionRepo = sessionRepo;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(
            EndSessionCommand request,
            CancellationToken ct)
        {
            var session = await _sessionRepo
                .GetQueryableAsync()
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseStaffs)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, ct);
            if (session == null)
                throw new ArgumentNullException($"Session {request.SessionId} not found.");

            if (session.Status != SessionStatus.Open)
                throw new ArgumentException("Session is not open.");

            var userId = _currentUser.UserId
                         ?? throw new UnauthorizedAccessException();
            var course = session.Course;
            var canManage = course.TeacherId == userId
                || course.CourseStaffs.Any(cs => cs.StaffId == userId)
                || _currentUser.IsInRole(Roles.ItStaff.ToString());
            if (!canManage)
                throw new UnauthorizedAccessException();

            session.Status = SessionStatus.Closed;
            session.EndTime = TimeOnly.FromDateTime(DateTime.Now);
            await _sessionRepo.UpdateAsync(session);

            return Unit.Value;
        }
    }
}