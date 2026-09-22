using Matgar.Application.Abstractions.Services;
using Matgar.Application.Features.Payments.PaymentDtos;
using Matgar.Infrastructure.Otions;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Matgar.Infrastructure.Payments
{
    internal class PaymobPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobSettings _options;

        public PaymobPaymentGateway(HttpClient httpClient, IOptions<PaymobSettings> paymobSettings)
        {
            _httpClient = httpClient;
            _options = paymobSettings.Value;
        }

        public async Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken)
        {
            var amountCents = checked(
       (long)Math.Round(
           request.Amount * 100,
           MidpointRounding.AwayFromZero));

            var payload = new
            {
                amount = amountCents,

                currency = request.Currency,

                payment_methods = new[]
                {
                     _options.IntegrationId
                },

                items = request.Items.Select(item => new
                {
                    name = item.Name,

                    amount = checked(
                        (long)Math.Round(
                            item.Amount * 100,
                            MidpointRounding.AwayFromZero)),

                    description = item.Description,

                    quantity = item.Quantity
                }),

                billing_data = new
                {
                    apartment = "NA",
                    first_name = request.CustomerFirstName,
                    last_name = request.CustomerLastName,
                    street = "NA",
                    building = "NA",
                    phone_number = request.CustomerPhone,
                    city = "NA",
                    country = "EG",
                    email = request.CustomerEmail,
                    floor = "NA",
                    state = "NA"
                },

                special_reference =
                    request.OrderReference,

                notification_url =
                    _options.NotificationUrl,

                redirection_url =
                    _options.RedirectionUrl
            };

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "v1/intention/");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Token",
                    _options.SecretKey);

            httpRequest.Content =
                JsonContent.Create(payload);

            using var response =
                await _httpClient.SendAsync(
                    httpRequest,
                    cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Paymob returned {(int)response.StatusCode}: " +
                    responseBody);
            }

            var paymobResponse =
                JsonSerializer.Deserialize<PaymobIntentionResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                ?? throw new InvalidOperationException(
                    "Invalid Paymob response.");

            return new PaymentResult
            {
                IntentionId =
                    paymobResponse.Id,

                PaymobOrderId =
                    paymobResponse.IntentionOrderId,

                ClientSecret =
                    paymobResponse.ClientSecret,

                CheckoutUrl =
                    $"https://accept.paymob.com/unifiedcheckout/" +
                    $"?publicKey={_options.PublicKey}" +
                    $"&clientSecret={paymobResponse.ClientSecret}"
            };
        }

        public Task<PaymentWebhookResult?> VerifyWebhookAsync(
      string payload, string hmac, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(hmac))
                return Task.FromResult<PaymentWebhookResult?>(null);

            JsonDocument doc;
            try { doc = JsonDocument.Parse(payload); }
            catch (JsonException) { return Task.FromResult<PaymentWebhookResult?>(null); }

            using (doc)
            {
                // Paymob transaction callbacks: { "type": "TRANSACTION", "obj": { ... } }
                if (!doc.RootElement.TryGetProperty("obj", out var obj))
                    return Task.FromResult<PaymentWebhookResult?>(null);

                // 1) Concatenate values in the exact order Paymob documents (lexicographic by key)
                var concatenated = string.Concat(
                    Value(obj, "amount_cents"),
                    Value(obj, "created_at"),
                    Value(obj, "currency"),
                    Value(obj, "error_occured"),
                    Value(obj, "has_parent_transaction"),
                    Value(obj, "id"),
                    Value(obj, "integration_id"),
                    Value(obj, "is_3d_secure"),
                    Value(obj, "is_auth"),
                    Value(obj, "is_capture"),
                    Value(obj, "is_refunded"),
                    Value(obj, "is_standalone_payment"),
                    Value(obj, "is_voided"),
                    Value(obj, "order", "id"),
                    Value(obj, "owner"),
                    Value(obj, "pending"),
                    Value(obj, "source_data", "pan"),
                    Value(obj, "source_data", "sub_type"),
                    Value(obj, "source_data", "type"),
                    Value(obj, "success"));

                // 2) HMAC-SHA512
                var keyBytes = Encoding.UTF8.GetBytes(_options.HmacSecret);
                var dataBytes = Encoding.UTF8.GetBytes(concatenated);
                var computed = Convert.ToHexString(HMACSHA512.HashData(keyBytes, dataBytes));

                // 3) Constant-time comparison
                var expected = Encoding.UTF8.GetBytes(computed.ToLowerInvariant());
                var actual = Encoding.UTF8.GetBytes(hmac.Trim().ToLowerInvariant());

                if (!CryptographicOperations.FixedTimeEquals(expected, actual))
                    return Task.FromResult<PaymentWebhookResult?>(null);

                // 4) Map
                var result = new PaymentWebhookResult
                {
                    TransactionId = obj.GetProperty("id").GetInt64(),
                    ProviderOrderId = obj.GetProperty("order").GetProperty("id").GetInt64(),
                    OrderReference = ExtractOrderReference(obj),
                    AmountCents = obj.GetProperty("amount_cents").GetInt64(),
                    Currency = obj.GetProperty("currency").GetString() ?? string.Empty,
                    Success = obj.GetProperty("success").GetBoolean(),
                    Pending = obj.GetProperty("pending").GetBoolean(),
                    IsRefunded = obj.GetProperty("is_refunded").GetBoolean(),
                    IsVoided = obj.GetProperty("is_voided").GetBoolean(),
                    IsAuth = obj.GetProperty("is_auth").GetBoolean(),
                    IsCapture = obj.GetProperty("is_capture").GetBoolean()
                };

                return Task.FromResult<PaymentWebhookResult?>(result);
            }
        }

        // Paymob echoes special_reference in different places depending on the flow.
        // Verify against a real sandbox payload and keep only the one that applies.
        private static string ExtractOrderReference(JsonElement obj)
        {
            if (obj.TryGetProperty("order", out var order) &&
                order.TryGetProperty("merchant_order_id", out var moid) &&
                moid.ValueKind is JsonValueKind.String)
                return moid.GetString() ?? string.Empty;

            return string.Empty;
        }

        // Returns the value as Paymob expects it in the HMAC string ("true"/"false" lowercase, empty for null)
        private static string Value(JsonElement root, params string[] path)
        {
            var cur = root;
            foreach (var p in path)
                if (cur.ValueKind != JsonValueKind.Object || !cur.TryGetProperty(p, out cur))
                    return string.Empty;

            return cur.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
                JsonValueKind.String => cur.GetString() ?? string.Empty,
                _ => cur.GetRawText()
            };
        }
    }
}
