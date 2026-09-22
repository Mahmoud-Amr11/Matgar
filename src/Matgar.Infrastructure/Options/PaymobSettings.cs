using System.ComponentModel.DataAnnotations;

namespace Matgar.Infrastructure.Otions
{
    public sealed class PaymobSettings
    {
        public const string SectionName = "PaymobSettings";

        [Required, Url]
        public string BaseUrl { get; init; } = "https://accept.paymob.com/";

        [Required, MinLength(10)]
        public string SecretKey { get; init; } = default!;


        public string PublicKey { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int IntegrationId { get; init; }

        [Required, MinLength(10)]
        public string HmacSecret { get; init; } = default!;

        [Required, Url]
        public string NotificationUrl { get; init; } = default!;

        [Required, Url]
        public string RedirectionUrl { get; init; } = default!;
    }
}