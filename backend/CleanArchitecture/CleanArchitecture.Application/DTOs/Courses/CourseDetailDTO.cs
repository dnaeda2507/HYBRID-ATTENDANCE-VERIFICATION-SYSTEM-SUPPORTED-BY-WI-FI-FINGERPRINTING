using System;
using System.Collections.Generic;
using CleanArchitecture.Application.DTOs.Departmant;
using CleanArchitecture.Application.DTOs.Lectures;
using CleanArchitecture.Application.DTOs.Users;

namespace CleanArchitecture.Application.DTOs.Courses
{
    public class CourseDetailDTO
    {
        public int Id { get; set; }
        public LectureDTO Lecture { get; set; }
        public UserDetailDTO Teacher { get; set; }
        public DepartmantLookupDTO Department { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Location { get; set; }
        public List<UserDetailDTO> CourseStaffs { get; set; } = new List<UserDetailDTO>();
        public List<UserDetailDTO> CourseStudents { get; set; } = new List<UserDetailDTO>();
    }
}