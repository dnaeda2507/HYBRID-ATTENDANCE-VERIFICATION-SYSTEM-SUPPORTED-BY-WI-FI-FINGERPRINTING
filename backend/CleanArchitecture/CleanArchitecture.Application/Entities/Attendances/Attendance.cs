using System;
using CleanArchitecture.Core.Entities.Sessions;

namespace CleanArchitecture.Core.Entities.Attendances
{
    public class Attendance
    {
        public int SessionId { get; set; }
        public Session Session { get; set; }
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
        public DateTime MarkedAtUtc { get; set; }
    }
}