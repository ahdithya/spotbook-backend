using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Application.DTOs.Request.Auth;
using SpotBook.Application.DTOs.Response.Auth;

namespace SpotBook.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}