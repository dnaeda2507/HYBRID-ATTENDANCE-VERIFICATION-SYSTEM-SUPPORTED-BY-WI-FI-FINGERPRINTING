using System;
using System.Collections.Generic;

namespace CleanArchitecture.Application.DTOs.Wifi
{
    public class StudentWifiScanDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int SessionId { get; set; }
        public DateTime ScannedAtUtc { get; set; }
        public List<AccessPointDto> AccessPoints { get; set; } = new List<AccessPointDto>();
        public double? ConfidenceScore { get; set; }
        public int? PredictedClassroomId { get; set; }
    }
}
