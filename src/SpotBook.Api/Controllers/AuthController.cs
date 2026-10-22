using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Models;
using SpotBook.Application.DTOs.Request.Auth;
using SpotBook.Application.DTOs.Response.Auth;
using SpotBook.Application.Interfaces.Auth;

namespace SpotBook.Api.Controllers;

public class AuthController(IAuthService authService) : BaseApiController
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        return HandleSuccess(response, "Registrasi berhasil.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return HandleSuccess(response, "Login berhasil.");
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return HandleSuccess(response, "Token berhasil diperbarui.");
    }
}
