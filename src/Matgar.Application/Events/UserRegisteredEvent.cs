namespace Matgar.Application.Events
{
    public sealed record UserRegisteredEvent(string UserId, string Email, string EmailConfirmationToken);
}
