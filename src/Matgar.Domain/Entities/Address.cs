namespace Matgar.Domain.Entities
{
    public class Address : BaseAuditEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;


    }
}