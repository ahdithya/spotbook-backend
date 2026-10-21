using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Payment
{
    public class PaymentWebhookPayload
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;
        [JsonPropertyName("status_code")]
        public string StatusCode { get; set; } = string.Empty;
        [JsonPropertyName("gross_amount")]
        public string GrossAmount { get; set; } = string.Empty;
        [JsonPropertyName("transaction_status")]
        public string TransactionStatus { get; set; } = string.Empty;
        [JsonPropertyName("signature_key")]
        public string SignatureKey { get; set; } = string.Empty;
    }
}