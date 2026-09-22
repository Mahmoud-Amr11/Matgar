namespace Matgar.Application.Abstractions.Email
{
    public interface IAppMailService
    {
        Task SendEmailConfirmationAsync(string userId, string email, string token);
    }
}
