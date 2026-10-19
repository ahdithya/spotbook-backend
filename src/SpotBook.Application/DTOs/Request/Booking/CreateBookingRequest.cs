using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Booking
{
    public class CreateBookingRequest
    {
        public Guid SpaceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? VoucherCode { get; set; }
    }
}