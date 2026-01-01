using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultStudent
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                Email = "student@gmail.com",
                FirstName = "Student",
                LastName = "Joe",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "student@gmail.com"
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    var result = await userManager.CreateAsync(defaultUser, "1357Abc.");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(defaultUser, Roles.Student.ToString());
                    }
                }

            }
        }
    }
}
