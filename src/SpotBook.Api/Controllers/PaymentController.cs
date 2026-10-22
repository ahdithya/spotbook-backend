using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Models;
using SpotBook.Application.DTOs.Request.Payment;
using SpotBook.Application.DTOs.Response.Payment;
using SpotBook.Application.Interfaces.Payments;

namespace SpotBook.Api.Controllers;

public class PaymentController(IPaymentService paymentService) : BaseApiController
{
    private readonly IPaymentService _paymentService = paymentService;

    [Authorize]
    [HttpPost("create-invoice")]
    public async Task<ActionResult<ApiResponse<PaymentInvoiceResponse>>> CreateInvoice([FromBody] CreatePaymentInvoiceRequest request)
    {
        var result = await _paymentService.CreateInvoiceAsync(CurrentUserId, request);
        return HandleSuccess(result, "Tagihan pembayaran berhasil dibuat.");
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<ActionResult<ApiResponse<object>>> HandleWebhook([FromBody] PaymentWebhookPayload payload)
    {
        await _paymentService.ProcessWebhookAsync(payload);
        return HandleSuccess("Webhook berhasil diproses.");
    }
}
