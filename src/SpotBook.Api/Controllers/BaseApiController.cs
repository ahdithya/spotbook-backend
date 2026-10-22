using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Models;

namespace SpotBook.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Identitas pengguna tidak valid atau sesi telah berakhir.");
            }
            return userId;
        }
    }

    protected ActionResult<ApiResponse<T>> HandleSuccess<T>(T data, string message = "Operasi berhasil.")
    {
        return Ok(ApiResponse<T>.SuccessResult(data, message));
    }

    protected ActionResult<ApiResponse<object>> HandleSuccess(string message = "Operasi berhasil.")
    {
        return Ok(ApiResponse<object>.SuccessResult(null!, message));
    }

    protected ActionResult<ApiResponse<T>> HandleCreated<T>(string actionName, object routeValues, T data, string message = "Data berhasil dibuat.")
    {
        return CreatedAtAction(actionName, routeValues, ApiResponse<T>.SuccessResult(data, message));
    }

    protected ActionResult<ApiResponse<T>> HandleError<T>(string message, int statusCode = StatusCodes.Status400BadRequest, List<string>? errors = null)
    {
        return StatusCode(statusCode, ApiResponse<T>.FailureResult(message, errors));
    }
}
