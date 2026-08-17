using Matgar.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Matgar.Infrastructure.Persistence.Seeders
{
    internal class AdminSeeder : IDataSeeder
    {
        public int Order => 2;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminSeeder(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var email = "Admin@Admin.com";

            var exists =
           await _userManager.FindByEmailAsync(email);


            if (exists is not null)
                return;


            var user = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Admin",

                Email = email,
                UserName = email,
                EmailConfirmed = true
            };


            await _userManager.CreateAsync(
                user,
                "Admin@12345");


            await _userManager.AddToRoleAsync(
                user,
                "Admin");
        }
    }
}
