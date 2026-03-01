using System;
using System.Collections.Generic;
using CleanArchitecture.Core.Entities.Sessions;

namespace CleanArchitecture.Core.Entities.Wifi
{
    public class StudentWifiScan : AuditableBaseEntity
    {
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }

        public DateTime ScannedAtUtc { get; set; }

        public ICollection<StudentWifiAccessPoint> AccessPoints { get; set; } = new List<StudentWifiAccessPoint>();

        public int? PredictedClassroomId { get; set; }
        public double? ConfidenceScore { get; set; }
    }
}