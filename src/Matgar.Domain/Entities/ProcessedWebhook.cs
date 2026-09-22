namespace Matgar.Domain.Entities
{
    public class ProcessedWebhook
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventKey { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
