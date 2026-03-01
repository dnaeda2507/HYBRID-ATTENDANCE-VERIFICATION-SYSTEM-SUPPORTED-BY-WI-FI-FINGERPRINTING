using System;
using System.Collections.Generic;

namespace CleanArchitecture.Application.DTOs.Wifi
{
    public class StudentWifiScanCreateDto
    {
        public string StudentId { get; set; }
        public int SessionId { get; set; }
        public DateTime ScannedAtUtc { get; set; }
        public List<AccessPointDto> AccessPoints { get; set; } = new List<AccessPointDto>();
    }
}
