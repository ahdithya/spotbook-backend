using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Payment
{
    public class CreatePaymentInvoiceRequest
    {
        public Guid BookingId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}