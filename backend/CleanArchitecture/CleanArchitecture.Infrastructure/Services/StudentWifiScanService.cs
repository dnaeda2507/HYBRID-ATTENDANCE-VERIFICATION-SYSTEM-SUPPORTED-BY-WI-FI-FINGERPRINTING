using System;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Wifi;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Core.Entities.Attendances;
using CleanArchitecture.Core.Entities.Sessions;
using CleanArchitecture.Core.Entities.Wifi;
using CleanArchitecture.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Services
{
    public class StudentWifiScanService : IStudentWifiScanService
    {
        private readonly IStudentWifiScanRepositoryAsync _repo;
        private readonly IWifiPredictionService _mlService;
        private readonly IMapper _mapper;
        private readonly ISessionRepositoryAsync _sessionRepo;
        private readonly IAttendanceRepositoryAsync _attendanceRepo;
        private readonly ICurrentUserService _currentUser;

        private const double DefaultConfidenceThreshold = 0.75;

        public StudentWifiScanService(
            IStudentWifiScanRepositoryAsync repo,
            IWifiPredictionService mlService,
            IMapper mapper,
            ISessionRepositoryAsync sessionRepo,
            IAttendanceRepositoryAsync attendanceRepo,
            ICurrentUserService currentUser)
        {
            _repo = repo;
            _mlService = mlService;
            _mapper = mapper;
            _sessionRepo = sessionRepo;
            _attendanceRepo = attendanceRepo;
            _currentUser = currentUser;
        }

        public async Task<int> CreateAsync(StudentWifiScanCreateDto dto)
        {
            if (dto == null || dto.AccessPoints == null || dto.AccessPoints.Count == 0)
                throw new ArgumentException("Access points cannot be null or empty");

            // 1️⃣ DTO → Entity
            var entity = _mapper.Map<StudentWifiScan>(dto);

            // 2️⃣ ML Prediction çağrısı
            try
            {
                var (predictedClassroomId, confidence) = await _mlService.PredictAsync(dto.AccessPoints);
                entity.PredictedClassroomId = predictedClassroomId;
                entity.ConfidenceScore = confidence;
            }
            catch
            {
                // Hata durumunda loglama / fallback
                entity.PredictedClassroomId = null;
                entity.ConfidenceScore = null;
            }

            // 3️⃣ Entity → DB kaydı
            var created = await _repo.AddAsync(entity);

            // 4️⃣ Attendance kararını ver
            try
            {
                // Session + Course bilgisiyle birlikte çek
                var session = await _sessionRepo
                    .GetQueryableAsync(nameof(Session.Course))
                    .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

                if (session != null &&
                    session.Course?.ClassroomId != null &&
                    entity.PredictedClassroomId.HasValue &&
                    entity.ConfidenceScore.HasValue)
                {
                    var predicted = entity.PredictedClassroomId.Value;
                    var confidence = entity.ConfidenceScore.Value;

                    // Session seviyesindeki Wi‑Fi ayarlarını dikkate al
                    var threshold = session.WifiVerificationRequired
                        ? session.MinimumConfidence
                        : DefaultConfidenceThreshold;

                    // ML tahmini ile dersin gerçek sınıfı eşleşiyorsa ve güven eşiği sağlanıyorsa yoklamayı otomatik işaretle
                    if (predicted == session.Course.ClassroomId && confidence >= threshold)
                    {
                        var studentId = dto.StudentId ?? _currentUser.UserId
                                       ?? throw new InvalidOperationException("StudentId not available");

                        // Aynı session için daha önce yoklama alınmış mı kontrol et
                        var exists = await _attendanceRepo
                            .GetQueryableAsync()
                            .FirstOrDefaultAsync(a => a.SessionId == dto.SessionId && a.StudentId == studentId);

                        if (exists == null)
                        {
                            var attendance = new Attendance
                            {
                                SessionId = dto.SessionId,
                                StudentId = studentId,
                                MarkedAtUtc = DateTime.UtcNow,
                                WifiConfidenceScore = confidence,
                                IsWifiVerified = true
                            };

                            await _attendanceRepo.AddAsync(attendance);
                        }
                    }
                }
            }
            catch
            {
                // Attendance kısmında hata olsa bile taramanın kendisi başarılı sayılmalı
            }

            return created.Id;
        }

        public async Task<StudentWifiScanDto> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<StudentWifiScanDto>(entity);
        }
    }
}