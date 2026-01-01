namespace CleanArchitecture.Core.Entities.Courses
{
    public class CourseStaff
    {
        public int CourseId { get; set; }
        public string StaffId { get; set; }
        public ApplicationUser Staff { get; set; }

        public CourseStaff(int courseId, string staffId)
        {
            CourseId = courseId;
            StaffId = staffId;
        }
    }
}