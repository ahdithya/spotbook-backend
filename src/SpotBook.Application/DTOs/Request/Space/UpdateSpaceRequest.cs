using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Space
{
    public class UpdateSpaceRequest
    {
        [Required(ErrorMessage = "Nama ruangan wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Nama ruangan maksimal 100 karakter.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter.")]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Kapasitas ruangan minimal 1 orang.")]
        public int Capacity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Harga sewa per jam harus lebih besar dari 0.")]
        public decimal PricePerHour { get; set; }

        public bool IsActive { get; set; }
    }
}