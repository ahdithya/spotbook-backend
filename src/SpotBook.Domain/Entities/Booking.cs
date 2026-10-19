using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Domain.Enums;

namespace SpotBook.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string BookingCode { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid SpaceId { get; set; }
        public Space Space { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public decimal BasePrice { get; set; }
        public Guid? VoucherId { get; set; }
        public Voucher? Voucher { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Status: "PendingPayment", "Paid", "Cancelled", "Completed"
        public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public PaymentTransaction? Payment { get; set; }
    }
}