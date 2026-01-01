using System.Collections.Generic;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Entities.Departmants;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? DepartmantId { get; set; }
        public Departmant Departmant { get; set; }
        public string InformationMail { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public bool OwnsToken(string token)
        {
            return RefreshTokens?.Find(x => x.Token == token) != null;
        }
    }
}
