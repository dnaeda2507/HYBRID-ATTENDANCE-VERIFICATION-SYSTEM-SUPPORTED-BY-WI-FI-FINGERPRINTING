using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CleanArchitecture.Core.Entities.Departmants;
using CleanArchitecture.Core.Entities.Lectures;

namespace CleanArchitecture.Core.Entities.Courses
{
    public class Course : AuditableBaseEntity
    {
        public int LectureId { get; set; }
        public Lecture Lecture { get; set; }
        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }
        public int DepartmantId { get; set; }
        public Departmant Departmant { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        [NotMapped]
        public TimeSpan EndTime => StartTime.Add(Duration);
        public string Location { get; set; }
        public List<CourseStaff> CourseStaffs { get; set; } = new List<CourseStaff>();
        public List<CourseStudent> CourseStudents { get; set; } = new List<CourseStudent>();

        public Course()
        {
        }

        public Course(int lectureId, string teacherId, int departmantId, DateTime schedule, string location, TimeSpan duration)
        {
            LectureId = lectureId;
            TeacherId = teacherId;
            DepartmantId = departmantId;
            DayOfWeek = schedule.DayOfWeek;
            StartTime = schedule.TimeOfDay;
            Duration = duration;
            Location = location;
        }

        public void AddCourseStaff(string staffId)
        {
            if (CourseStaffs == null)
            {
                CourseStaffs = new List<CourseStaff>();
            }

            if (CourseStaffs.Find(x => x.StaffId == staffId) == null)
            {
                CourseStaffs.Add(new CourseStaff
                (
                    Id,
                    staffId
                ));
            }

        }

        public void AddCourseStudent(string studentId)
        {
            if (CourseStudents == null)
            {
                CourseStudents = new List<CourseStudent>();
            }

            if (CourseStudents.Find(x => x.StudentId == studentId) == null)
            {
                CourseStudents.Add(new CourseStudent
                (
                    Id,
                    studentId
                ));
            }
        }
    }
}