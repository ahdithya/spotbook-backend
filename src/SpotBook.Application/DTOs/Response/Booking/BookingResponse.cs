using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Response.Booking
{
    public class BookingResponse
    {
        public Guid Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public Guid SpaceId { get; set; }
        public string SpaceName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal DurationInHours { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}