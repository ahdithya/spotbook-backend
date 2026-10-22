using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Models;

namespace SpotBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> HandleSuccess<T>(T data, string message = "Berhasil.")
        {
            return Ok(ApiResponse<T>.SuccessResult(data, message));
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
}