namespace Matgar.Application.Abstractions.Services
{
    public interface IAppMailService
    {
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
    }
}
