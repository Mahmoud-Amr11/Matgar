using MailKit.Net.Smtp;
using Matgar.Application.Abstractions.Services;
using Matgar.Infrastructure.Otions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
namespace Matgar.Infrastructure.Services
{
    internal class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;

        public EmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public async Task SendAsync(string to, string subject, string body, List<IFormFile>? attachments = null)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailOptions.DisplayName, _emailOptions.Email));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;


            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            if (attachments is not null)
            {
                foreach (var file in attachments)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    builder.Attachments.Add(
                        file.FileName,
                        ms.ToArray(),
                        ContentType.Parse(file.ContentType));
                }
            }


            email.Body = builder.ToMessageBody();


            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_emailOptions.Host, _emailOptions.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailOptions.Email, _emailOptions.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
