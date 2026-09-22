using MailKit.Net.Smtp;
using Matgar.Application.Abstractions.Email;
using Matgar.Infrastructure.Otions;
using Microsoft.Extensions.Options;
using MimeKit;
namespace Matgar.Infrastructure.Email
{
    internal class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;

        public EmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailOptions.DisplayName, _emailOptions.Email));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;


            var builder = new BodyBuilder
            {
                HtmlBody = body
            };



            email.Body = builder.ToMessageBody();


            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_emailOptions.Host, _emailOptions.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailOptions.Email, _emailOptions.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
