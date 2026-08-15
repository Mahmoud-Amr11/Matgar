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

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {

            var subject = "Confirm your email";

            var body = $"""
                        <h2>Email Confirmation</h2>
                        <p>Please confirm your email by clicking the link below:</p>
                        <p><a href="{confirmationLink}">Confirm Email</a></p>
                        """;

            await _mailService.SendAsync(email, subject, body);
        }
    }
}
