using System.Text.Json.Serialization;

namespace Matgar.Application.Features.Payments.PaymentDtos
{
    public sealed class PaymobIntentionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; init; } = string.Empty;

        [JsonPropertyName("intention_order_id")]
        public long IntentionOrderId { get; init; }

        [JsonPropertyName("special_reference")]
        public string SpecialReference { get; init; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; init; } = string.Empty;
    }
}
