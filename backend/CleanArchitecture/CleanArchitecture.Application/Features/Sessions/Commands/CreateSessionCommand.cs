using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Courses;
using CleanArchitecture.Core.Entities.Sessions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace CleanArchitecture.Application.Features.Attendances.Commands
{
    public partial class CreateSessionCommand : IRequest<CreateSessionResult>
    {
        public int CourseId { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }

    public class CreateSessionResult
    {
        public int SessionId { get; set; }
        public string Token { get; set; }
        public string QrCodeDataUri { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class CreateSessionCommandHandler
        : IRequestHandler<CreateSessionCommand, CreateSessionResult>
    {
        private static readonly char[] _tokenChars =
            "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

        private readonly ICourseRepositoryAsync _courseRepo;
        private readonly ISessionRepositoryAsync _sessionRepo;
        private readonly ICurrentUserService _currentUser;

        public CreateSessionCommandHandler(
            ICourseRepositoryAsync courseRepo,
            ISessionRepositoryAsync sessionRepo,
            ICurrentUserService currentUser)
        {
            _courseRepo = courseRepo;
            _sessionRepo = sessionRepo;
            _currentUser = currentUser;
        }

        public async Task<CreateSessionResult> Handle(
            CreateSessionCommand request,
            CancellationToken ct)
        {
            var course = await _courseRepo
                .GetQueryableAsync(nameof(Course.CourseStaffs))
                .FirstOrDefaultAsync(c => c.Id == request.CourseId, ct);
            if (course == null)
                throw new ArgumentNullException($"Course {request.CourseId} not found.");

            var userId = _currentUser.UserId
                         ?? throw new UnauthorizedAccessException();
            var isOwner = course.TeacherId == userId;
            var isStaff = course.CourseStaffs.Any(cs => cs.StaffId == userId);
            if (!isOwner && !isStaff)
                throw new UnauthorizedAccessException();

            string token;
            using var rng = RandomNumberGenerator.Create();
            do
            {
                var bytes = new byte[6];
                rng.GetBytes(bytes);
                var sb = new StringBuilder(6);
                foreach (var b in bytes)
                    sb.Append(_tokenChars[b % _tokenChars.Length]);
                token = sb.ToString();
            }
            while (await _sessionRepo
                .GetQueryableAsync()
                .AnyAsync(s => s.Token == token && s.Status == SessionStatus.Open, ct));

            var nowUtc = DateTime.Now;
            var leftTimeToCourseEnd = nowUtc.Date + course.EndTime - nowUtc;
            var session = new Session
            {
                CourseId = course.Id,
                Date = nowUtc,
                StartTime = request.StartTime ?? TimeOnly.FromDateTime(nowUtc),
                EndTime = request.EndTime ?? TimeOnly.FromDateTime(nowUtc.Add(leftTimeToCourseEnd)),
                Token = token,
                Status = SessionStatus.Open
            };
            await _sessionRepo.AddAsync(session);

            string base64;
            var combinedToken = $"{session.Id}:{token}";
            using (var gen = new QRCodeGenerator())
            using (var data = gen.CreateQrCode(combinedToken, QRCodeGenerator.ECCLevel.Q))
            using (var qr = new PngByteQRCode(data))
            {
                base64 = Convert.ToBase64String(qr.GetGraphic(20));
            }

            return new CreateSessionResult
            {
                SessionId = session.Id,
                Token = token,
                QrCodeDataUri = $"data:image/png;base64,{base64}",
                EndTime = session.EndTime
            };
        }
    }
}