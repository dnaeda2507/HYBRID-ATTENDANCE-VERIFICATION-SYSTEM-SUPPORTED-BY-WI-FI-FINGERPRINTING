using System.Collections.Generic;
using CleanArchitecture.Core.Enums;

namespace CleanArchitecture.Application.DTOs.Users
{
    public class UserDetailDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string InformationMail { get; set; }
        public List<Roles> Roles { get; set; }
    }
}