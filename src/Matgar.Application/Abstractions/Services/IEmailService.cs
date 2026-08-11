using Microsoft.AspNetCore.Http;

namespace Matgar.Application.Abstractions.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, List<IFormFile>? attachments = null);
    }
}
