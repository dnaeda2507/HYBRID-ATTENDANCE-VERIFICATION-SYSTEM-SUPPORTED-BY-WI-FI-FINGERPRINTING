namespace CleanArchitecture.Application.DTOs.Sessions
{
    public class TeacherSessionDTO
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
        public int AttendedStudentCount { get; set; }
    }
}
