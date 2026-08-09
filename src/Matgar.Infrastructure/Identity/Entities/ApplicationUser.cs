using Microsoft.AspNetCore.Identity;

namespace Matgar.Infrastructure.Identity.Entities
{
    internal class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;

    }
}
