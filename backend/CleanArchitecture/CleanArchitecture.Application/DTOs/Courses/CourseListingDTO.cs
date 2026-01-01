using System;

namespace CleanArchitecture.Application.DTOs.Courses
{
    public class CourseListingDTO
    {
        public int Id { get; set; }
        public string LectureName { get; set; }
        public string TeacherName { get; set; }
        public string DepartmentName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Location { get; set; }
    }
}