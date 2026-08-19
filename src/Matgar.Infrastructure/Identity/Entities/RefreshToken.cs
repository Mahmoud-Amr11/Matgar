namespace Matgar.Infrastructure.Identity.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string HashToken { get; set; } = null!;
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public bool IsExpired => DateTime.Now > ExpiresOn;
        public bool IsActive => RevokedOn == null && !IsExpired;
    }
}
