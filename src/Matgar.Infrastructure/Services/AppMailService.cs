using Matgar.Application.Abstractions.Services;

namespace Matgar.Infrastructure.Services
{
    internal class AppMailService : IAppMailService
    {
        private readonly IEmailService _mailService;

        public AppMailService(IEmailService mailService)
        {
            _mailService = mailService;
        }

        public async Task SendEmailConfirmationAsync(string userId, string email, string token)
        {

            var subject = "Confirm your email";
            var confirmationLink = $"https://localhost:7123/api/Auth/confirm-email?userId={userId}&token={token}";

            var body = $"""
            <h2>Email Confirmation</h2>
            <p>Please confirm your email by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm Email</a></p>
            """;


            await _mailService.SendAsync(email, subject, body);
        }
    }
}
