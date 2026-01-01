namespace CleanArchitecture.Application.DTOs.Attendances
{
    public class AttendedUserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string MarkedAtUtc { get; set; }
    }
}