namespace Matgar.Application.Abstractions.Persistence.Repositories
{
    public interface IProcessedWebhookRepository
    {
        Task<bool> TryRegisterAsync(string eventKey, CancellationToken cancellationToken = default);
    }
}
