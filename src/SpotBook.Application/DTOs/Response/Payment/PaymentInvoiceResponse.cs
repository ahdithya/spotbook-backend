using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Response.Payment
{
    public class PaymentInvoiceResponse
    {
        public Guid PaymentTransactionId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentGatewayRef { get; set; } = string.Empty; // Nomor Virtual Account atau ID Transaksi
        public string PaymentUrl { get; set; } = string.Empty;        // Link simulasi bayar
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}