using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Response.Auth
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}