using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Auth
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "AccessToken wajib diisi.")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "RefreshToken wajib diisi.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}