namespace CleanArchitecture.Core.Entities.Courses
{
    public class CourseStudent
    {
        public int CourseId { get; set; }
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        public CourseStudent(int courseId, string studentId)
        {
            CourseId = courseId;
            StudentId = studentId;
        }
    }
}