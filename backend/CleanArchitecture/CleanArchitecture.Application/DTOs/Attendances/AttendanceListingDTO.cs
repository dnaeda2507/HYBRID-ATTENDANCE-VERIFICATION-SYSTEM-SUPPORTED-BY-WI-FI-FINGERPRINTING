using System;

namespace CleanArchitecture.Application.DTOs.Attendances
{
    public class AttendanceListingDTO
    {
        public string CourseName { get; set; }
        public DateTime MarkedAtUtc { get; set; }
    }
}