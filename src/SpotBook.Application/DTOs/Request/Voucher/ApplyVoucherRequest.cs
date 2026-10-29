using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Voucher
{
    public class ApplyVoucherRequest
    {
        [Required(ErrorMessage = "Kode voucher wajib diisi.")]
        public string Code { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Nilai transaksi harus lebih besar dari 0.")]
        public decimal OrderAmount { get; set; }
    }
}