using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Booking
{
    public class CreateBookingRequest
    {
        [Required(ErrorMessage = "SpaceId wajib diisi.")]
        public Guid SpaceId { get; set; }

        [Required(ErrorMessage = "Waktu mulai sewa wajib diisi.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Waktu selesai sewa wajib diisi.")]
        public DateTime EndTime { get; set; }

        public string? VoucherCode { get; set; }
    }
}