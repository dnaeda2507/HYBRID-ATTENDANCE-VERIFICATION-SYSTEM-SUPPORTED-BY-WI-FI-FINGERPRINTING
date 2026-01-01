using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultItStaff
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                Email = "admin@gmail.com",
                FirstName = "Test",
                LastName = "ItStaff",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "admin@gmail.com"
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    var result = await userManager.CreateAsync(defaultUser, "1357Abc.");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(defaultUser, Roles.ItStaff.ToString());
                    }
                }

            }
        }
    }
}
