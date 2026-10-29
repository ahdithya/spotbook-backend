using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Domain.Enums;

namespace SpotBook.Application.DTOs.Request.Voucher
{
    public class CreateVoucherRequest
    {
        [Required(ErrorMessage = "Kode voucher wajib diisi.")]
        [MaxLength(50, ErrorMessage = "Kode voucher maksimal 50 karakter.")]
        public string Code { get; set; } = string.Empty;

        public DiscountType DiscountType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Nilai diskon harus lebih besar dari 0.")]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maksimal diskon tidak boleh negatif.")]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimal transaksi tidak boleh negatif.")]
        public decimal MinOrderAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kuota voucher minimal 1.")]
        public int Quota { get; set; }

        [Required(ErrorMessage = "Tanggal berlaku mulai wajib diisi.")]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Tanggal berlaku selesai wajib diisi.")]
        public DateTime ValidUntil { get; set; }
    }
}