namespace Matgar.Application.Abstractions.Services
{
    public interface IAppMailService
    {
        Task SendEmailConfirmationAsync(string userId, string email, string token);
    }
}
