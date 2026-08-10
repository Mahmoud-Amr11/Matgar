using Microsoft.AspNetCore.Identity;

namespace Matgar.Infrastructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;

        public ICollection<RefreshToken> RefreshTokens { get; set; }

    }
}
