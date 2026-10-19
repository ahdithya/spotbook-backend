using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Domain.Enums;

namespace SpotBook.Domain.Entities
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public string PaymentGatewayRef { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid; // "Unpaid", "Settled", "Expired"
        public string? PaymentUrl { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}