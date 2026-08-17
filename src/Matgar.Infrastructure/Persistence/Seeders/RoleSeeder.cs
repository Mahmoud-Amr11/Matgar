using Microsoft.AspNetCore.Identity;

namespace Matgar.Infrastructure.Persistence.Seeders
{
    internal class RoleSeeder : IDataSeeder
    {
        public int Order => 1;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleSeeder(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            var roles = new List<string> { "Admin", "User", "Customer", "Vendor" };

            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                    continue;

                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
