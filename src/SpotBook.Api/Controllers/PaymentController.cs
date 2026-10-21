using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.DTOs.Request.Payment;
using SpotBook.Application.DTOs.Response.Payment;
using SpotBook.Application.Interfaces.Payments;

namespace SpotBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(IPaymentService paymentService) : ControllerBase
    {

        private readonly IPaymentService _paymentService = paymentService;
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Identitas pengguna tidak valid.");
            }
            return userId;
        }

        [Authorize]
        [HttpPost("create-invoice")]
        public async Task<ActionResult<PaymentInvoiceResponse>> CreateInvoice([FromBody] CreatePaymentInvoiceRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _paymentService.CreateInvoiceAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] PaymentWebhookPayload payload)
        {
            try
            {
                await _paymentService.ProcessWebhookAsync(payload);
                return Ok(new { message = "Webhook berhasil diproses." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}