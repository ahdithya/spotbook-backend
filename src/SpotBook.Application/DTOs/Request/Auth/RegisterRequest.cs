using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [MaxLength(150, ErrorMessage = "Email maksimal 150 karakter.")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Nomor telepon maksimal 20 digit.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [MinLength(6, ErrorMessage = "Password minimal terdiri dari 6 karakter.")]
        public string Password { get; set; } = string.Empty;
    }
}