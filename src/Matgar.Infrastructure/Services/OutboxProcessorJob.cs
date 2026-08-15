using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Abstractions.Services;
using Matgar.Application.Events;
using System.Text.Json;

namespace Matgar.Infrastructure.Services
{
    public class OutboxProcessorJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppMailService _emailSender;

        public OutboxProcessorJob(IUnitOfWork unitOfWork, IAppMailService emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public async Task ProcessOutboxMessages()
        {
            var messages = await _unitOfWork.OutboxMessages.FindAsync(m => m.ProcessedOn == null);

            foreach (var message in messages.OrderBy(m => m.OccurredOn).Take(20))
            {
                try
                {
                    if (message.Type == nameof(UserRegisteredEvent))
                    {
                        var evt = JsonSerializer.Deserialize<UserRegisteredEvent>(message.Content)!;
                        await _emailSender.SendEmailConfirmationAsync(evt.Email, evt.EmailConfirmationToken);
                    }

                    message.ProcessedOn = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.Error = ex.Message;
                    message.RetryCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

    }
}
