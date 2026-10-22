using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.Common.Models;

namespace SpotBook.Api.Middlewares
{
    public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Teruskan request ke controller
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan yang tidak tertangani: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }


        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = exception switch
            {
                NotFoundException => HttpStatusCode.NotFound,               // 404
                BadRequestException => HttpStatusCode.BadRequest,           // 400
                UnauthorizedAccessException => HttpStatusCode.Unauthorized, // 401
                _ => HttpStatusCode.InternalServerError                    // 500
            };
            context.Response.StatusCode = (int)statusCode;
            var response = ApiResponse<object>.FailureResult(
                exception.Message,
                errors: statusCode == HttpStatusCode.InternalServerError ? ["Internal Server Error"] : null
            );
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}