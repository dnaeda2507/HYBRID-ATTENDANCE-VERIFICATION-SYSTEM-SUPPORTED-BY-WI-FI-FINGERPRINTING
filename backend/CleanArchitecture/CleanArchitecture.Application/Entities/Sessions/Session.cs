using System;
using System.Collections.Generic;
using CleanArchitecture.Application.Enums;
using CleanArchitecture.Core.Entities.Attendances;
using CleanArchitecture.Core.Entities.Courses;

namespace CleanArchitecture.Core.Entities.Sessions
{
    public class Session : AuditableBaseEntity
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Token { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Open;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        public bool IsOpen()
        {
            var now = DateTime.Now;

            var sessionDate = Date.Date;

            var windowStart = sessionDate.Add(StartTime.ToTimeSpan());
            var windowEnd = sessionDate.Add(EndTime.ToTimeSpan());

            return now >= windowStart && now <= windowEnd
                   && Status == SessionStatus.Open;
        }
    }
}