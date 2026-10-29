using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Payment
{
    public class CreatePaymentInvoiceRequest
    {
        [Required(ErrorMessage = "BookingId wajib diisi.")]
        public Guid BookingId { get; set; }

        [Required(ErrorMessage = "Metode pembayaran wajib diisi.")]
        [MaxLength(50, ErrorMessage = "Metode pembayaran maksimal 50 karakter.")]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}