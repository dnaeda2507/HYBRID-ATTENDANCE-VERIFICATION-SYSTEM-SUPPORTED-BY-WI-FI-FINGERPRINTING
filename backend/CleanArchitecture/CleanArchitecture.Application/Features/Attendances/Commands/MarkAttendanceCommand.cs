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
    // Attendance methods enum
    public enum AttendanceMethod
    {
        QRCode = 0,      // Token ile
        Password = 1,    // Şifre ile (aynı Token)
        Wifi = 2         // WiFi ile
    }

    // ── Unified command for all attendance methods ──────────────────────────
    public partial class MarkAttendanceCommand : IRequest<Unit>
    {
        public int SessionId { get; set; }
        public string Token { get; set; }    // QR/Password için
        public AttendanceMethod Method { get; set; } = AttendanceMethod.QRCode;
        public bool IsSuspicious { get; set; } // Model disagreement flag
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
                .GetQueryableAsync()
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseStudents)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, ct);

            if (session is null)
                throw new ArgumentNullException($"Session {request.SessionId} not found.");
            
            // QR Code veya Şifre ile
            if (request.Method == AttendanceMethod.QRCode || request.Method == AttendanceMethod.Password)
            {
                if (!string.Equals(session.Token, request.Token, StringComparison.Ordinal))
                    throw new ArgumentException("Invalid attendance code.");
            }
            
            if (session.Status != SessionStatus.Open)
                throw new ArgumentException("Attendance session is not open.");
            if (session.IsOpen() == false)
                throw new ArgumentException("Not within attendance time window.");

            var studentId = _currentUser.UserId
                            ?? throw new UnauthorizedAccessException("Unauthenticated");

            // Zaten yoklama almış mı kontrol et
            var alreadyAttended = await _attendanceRepo
                .GetQueryableAsync()
                .AnyAsync(a => a.SessionId == session.Id && a.StudentId == studentId, ct);
            if (alreadyAttended)
                throw new ArgumentException("You have already marked attendance.");
            
            if (!session.Course.CourseStudents.Any(s => s.StudentId == studentId))
                throw new ArgumentException("You are not enrolled in this course.");

            await _attendanceRepo.AddAsync(new Attendance
            {
                SessionId = session.Id,
                StudentId = studentId,
                MarkedAtUtc = DateTime.UtcNow,
                IsWifiVerified = (request.Method == AttendanceMethod.Wifi),
                IsSuspicious = request.IsSuspicious
            });

            return Unit.Value;
        }
    }
}