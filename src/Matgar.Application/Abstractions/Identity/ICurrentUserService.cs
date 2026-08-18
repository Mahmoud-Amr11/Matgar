namespace Matgar.Application.Abstractions.Identity
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserEmail { get; }
        string? UserName { get; }
        IEnumerable<string> Roles { get; }
        bool IsAuthenticated { get; }
    }
}
