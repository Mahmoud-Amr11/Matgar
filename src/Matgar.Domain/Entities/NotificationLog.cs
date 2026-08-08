namespace Matgar.Domain.Entities
{
    public class NotificationLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? RelatedOrderId { get; set; }   
        public string Message { get; set; } = string.Empty;
        public bool IsSent { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}