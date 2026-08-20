using Matgar.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Matgar.Infrastructure.Identity.Services
{
    internal class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private ClaimsPrincipal? User => _contextAccessor.HttpContext?.User;
        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();



        public string? UserEmail => User?.FindFirstValue(ClaimTypes.Email);
        public string? UserName => User?.FindFirstValue(ClaimTypes.Name);

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

        public IEnumerable<string> Roles => User?.Claims
                    .Where(x => x.Type == ClaimTypes.Role)
                    .Select(x => x.Value) ?? [];
    }
}
