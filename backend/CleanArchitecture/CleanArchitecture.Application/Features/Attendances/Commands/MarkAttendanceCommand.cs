using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Entities.Attendances;
using CleanArchitecture.Core.Entities.Sessions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Attendances.Commands
{
    // ── Mevcut command (değişmedi) ──────────────────────────────────────────
    public partial class MarkAttendanceCommand : IRequest<Unit>
    {
        public int SessionId { get; set; }
        public string Token { get; set; }
    }

    public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Unit>
    {
        private readonly ISessionRepositoryAsync _sessionRepo;
        private readonly IAttendanceRepositoryAsync _attendanceRepo;
        private readonly ICurrentUserService _currentUser;

        public MarkAttendanceCommandHandler(
            ISessionRepositoryAsync sessionRepo,
            IAttendanceRepositoryAsync attendanceRepo,
            ICurrentUserService currentUser)
        {
            _sessionRepo = sessionRepo;
            _attendanceRepo = attendanceRepo;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(MarkAttendanceCommand request, CancellationToken ct)
        {
            var session = await _sessionRepo
                .GetQueryableAsync(nameof(Session.Attendances))
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseStudents)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, ct);

            if (session is null)
                throw new ArgumentNullException($"Session {request.SessionId} not found.");
            if (!string.Equals(session.Token, request.Token, StringComparison.Ordinal))
                throw new ArgumentException("Invalid attendance code.");
            if (session.Status != SessionStatus.Open)
                throw new ArgumentException("Attendance session is not open.");

            var nowUtc = DateTime.Now;
            if (session.IsOpen() == false)
                throw new ArgumentException("Not within attendance time window.");

            var studentId = _currentUser.UserId
                            ?? throw new UnauthorizedAccessException("Unauthenticated");

            if (session.Attendances.Any(a => a.StudentId == studentId))
                throw new ArgumentException("You have already marked attendance.");
            if (!session.Course.CourseStudents.Any(s => s.StudentId == studentId))
                throw new ArgumentException("You are not enrolled in this course.");

            await _attendanceRepo.AddAsync(new Attendance
            {
                SessionId = session.Id,
                StudentId = studentId,
                MarkedAtUtc = nowUtc
            });

            return Unit.Value;
        }
    }

    // ── YENİ: WiFi üzerinden öğrenci başlatır, token yerine konum doğrular ─
    public class MarkAttendanceByWifiCommand : IRequest<Unit>
    {
        public int SessionId { get; set; }
        // StudentId JWT'den otomatik gelir, dışarıdan set edilmez
    }

    public class MarkAttendanceByWifiCommandHandler : IRequestHandler<MarkAttendanceByWifiCommand, Unit>
    {
        private readonly ISessionRepositoryAsync _sessionRepo;
        private readonly IAttendanceRepositoryAsync _attendanceRepo;
        private readonly IAuthenticatedUserService _currentUser;

        public MarkAttendanceByWifiCommandHandler(
            ISessionRepositoryAsync sessionRepo,
            IAttendanceRepositoryAsync attendanceRepo,
            IAuthenticatedUserService currentUser)
        {
            _sessionRepo = sessionRepo;
            _attendanceRepo = attendanceRepo;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(MarkAttendanceByWifiCommand request, CancellationToken ct)
        {
            var session = await _sessionRepo
                .GetQueryableAsync(nameof(Session.Attendances))
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseStudents)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, ct);

            if (session is null)
                throw new ArgumentNullException($"Session {request.SessionId} not found.");
            if (session.Status != SessionStatus.Open)
                throw new ArgumentException("Attendance session is not open.");
            if (!session.IsOpen())
                throw new ArgumentException("Not within attendance time window.");

            var studentId = _currentUser.UserId
                            ?? throw new UnauthorizedAccessException("Unauthenticated");

            // Zaten kayıtlıysa sessizce geç
            if (session.Attendances.Any(a => a.StudentId == studentId))
                return Unit.Value;

            if (!session.Course.CourseStudents.Any(s => s.StudentId == studentId))
                throw new ArgumentException("Student is not enrolled in this course.");

            await _attendanceRepo.AddAsync(new Attendance
            {
                SessionId = session.Id,
                StudentId = studentId,
                MarkedAtUtc = DateTime.UtcNow,
                IsWifiVerified = true,
            });

            return Unit.Value;
        }
    }
}