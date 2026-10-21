using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Application.DTOs.Request.Payment;
using SpotBook.Application.DTOs.Response.Payment;

namespace SpotBook.Application.Interfaces.Payments
{
    public interface IPaymentService
    {
        Task<PaymentInvoiceResponse> CreateInvoiceAsync(Guid userId, CreatePaymentInvoiceRequest request);
        Task<bool> ProcessWebhookAsync(PaymentWebhookPayload payload);
    }
}