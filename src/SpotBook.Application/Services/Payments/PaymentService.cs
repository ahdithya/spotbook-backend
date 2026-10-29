using System;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Globalization;
using SpotBook.Application.Common.Interfaces;
using System.Text;
using System.Threading.Tasks;
using SpotBook.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotBook.Application.DTOs.Request.Payment;
using SpotBook.Application.DTOs.Response.Payment;
using SpotBook.Application.Interfaces;
using SpotBook.Application.Interfaces.Payments;
using SpotBook.Domain.Entities;
using SpotBook.Domain.Enums;
using System.Transactions;
using System.Text.Json;

namespace SpotBook.Application.Services.Payments
{
    public class PaymentService(
        IAppDbContext context,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ICacheService cacheService) : IPaymentService
    {
        private readonly IAppDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ICacheService _cacheService = cacheService;
        public async Task<PaymentInvoiceResponse> CreateInvoiceAsync(Guid userId, CreatePaymentInvoiceRequest request)
        {
            var booking = await _context.Bookings
                        .Include(b => b.User)
                        .Include(b => b.Payment)
                        .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == userId);

            if (booking == null)
            {
                throw new NotFoundException("Data booking tidak ditemukan.");
            }

            if (booking.Status != BookingStatus.PendingPayment)
            {
                throw new BadRequestException("Hanya pemesanan berstatus PendingPayment yang bisa dibuatkan tagihan pembayaran.");
            }

            // Jika sudah pernah dibuatkan invoice sebelumnya dan masih aktif
            if (booking.Payment != null)
            {
                if (booking.Payment.PaymentStatus != PaymentStatus.Expired)
                {
                    return new PaymentInvoiceResponse
                    {
                        PaymentTransactionId = booking.Payment.Id,
                        BookingId = booking.Id,
                        BookingCode = booking.BookingCode,
                        Amount = booking.Payment.Amount,
                        PaymentMethod = booking.Payment.PaymentMethod,
                        PaymentGatewayRef = booking.Payment.PaymentGatewayRef,
                        PaymentUrl = booking.Payment.PaymentUrl ?? string.Empty,
                        Status = booking.Payment.PaymentStatus.ToString(),
                        CreatedAt = booking.Payment.CreatedAt
                    };
                }

                // Jika invoice sebelumnya sudah Expired, perbarui BookingCode agar Midtrans menerima order_id baru
                booking.BookingCode = $"SB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
            }

            // kirim ke midtrans
            // get serverKey
            var serverKey = _configuration["PaymentSettings:ServerKey"]
                    ?? throw new InvalidOperationException("Midtrans ServerKey belum dikonfigurasi.");
            var snapUrL = _configuration["PaymentSettings:SnapUrl"];


            // payload for snap midtrans
            var snapPayload = new
            {
                transaction_details = new
                {
                    order_id = booking.BookingCode,
                    gross_amount = Convert.ToInt64(Math.Round(booking.TotalAmount))
                },
                customer_details = new
                {
                    first_name = booking.User.FullName,
                    email = booking.User.Email,
                    phone = booking.User.PhoneNumber
                }
            };

            // kirim request ke API midtrans
            var client = _httpClientFactory.CreateClient();

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{serverKey}:"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);


            var jsonContent = new StringContent(
                JsonSerializer.Serialize(snapPayload),
                Encoding.UTF8,
                "application/json"
            );



            var midtransResponse = await client.PostAsync(snapUrL, jsonContent);
            if (!midtransResponse.IsSuccessStatusCode)
            {
                var errorBody = await midtransResponse.Content.ReadAsStringAsync();
                throw new BadRequestException($"Gagal menghubungi Midtrans Snap API: {errorBody}");
            }

            var responseBody = await midtransResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseBody);




            var snapToken = jsonDoc.RootElement.GetProperty("token").GetString() ?? string.Empty;
            var redirectUrl = jsonDoc.RootElement.GetProperty("redirect_url").GetString() ?? string.Empty;

            if (booking.Payment != null)
            {
                booking.Payment.PaymentGatewayRef = snapToken;
                booking.Payment.PaymentMethod = request.PaymentMethod;
                booking.Payment.Amount = booking.TotalAmount;
                booking.Payment.PaymentStatus = PaymentStatus.Unpaid;
                booking.Payment.PaymentUrl = redirectUrl;
                booking.Payment.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var payment = new PaymentTransaction
                {
                    BookingId = booking.Id,
                    PaymentGatewayRef = snapToken,
                    PaymentMethod = request.PaymentMethod,
                    Amount = booking.TotalAmount,
                    PaymentStatus = PaymentStatus.Unpaid,
                    PaymentUrl = redirectUrl,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PaymentTransactions.Add(payment);
                booking.Payment = payment;
            }

            await _context.SaveChangesAsync();
            return new PaymentInvoiceResponse
            {
                PaymentTransactionId = booking.Payment.Id,
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                Amount = booking.Payment.Amount,
                PaymentMethod = booking.Payment.PaymentMethod,
                PaymentGatewayRef = booking.Payment.PaymentGatewayRef,
                PaymentUrl = booking.Payment.PaymentUrl ?? string.Empty,
                Status = booking.Payment.PaymentStatus.ToString(),
                CreatedAt = booking.Payment.CreatedAt
            };
        }
        public async Task<bool> ProcessWebhookAsync(PaymentWebhookPayload payload)
        {
            // 1. Verifikasi Signature Keamanan (SHA512)
            var serverKey = _configuration["PaymentSettings:ServerKey"]
                ?? throw new InvalidOperationException("Payment ServerKey belum dikonfigurasi.");
            // Rumus Resmi Midtrans: SHA512(order_id + status_code + gross_amount + ServerKey)
            string rawSignature = $"{payload.OrderId}{payload.StatusCode}{payload.GrossAmount}{serverKey}";
            string expectedSignature = ComputeSha512Hash(rawSignature);
            if (!expectedSignature.Equals(payload.SignatureKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Signature Webhook tidak valid! Kemungkinan ada manipulasi data.");
            }
            // 2. Cari Booking berdasarkan BookingCode (OrderId)
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingCode == payload.OrderId);
            if (booking == null)
            {
                throw new NotFoundException($"Booking dengan kode {payload.OrderId} tidak ditemukan.");
            }
            // 3. Verifikasi Gross Amount dengan TotalAmount Booking
            if (!decimal.TryParse(payload.GrossAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var webhookGrossAmount) ||
                Math.Round(webhookGrossAmount, 2) != Math.Round(booking.TotalAmount, 2))
            {
                throw new BadRequestException("Gross amount pada webhook tidak sesuai dengan total tagihan pemesanan.");
            }

            // 4. Idempotency Check: Jika status sudah Paid, jangan proses berulang kali
            if (booking.Status == BookingStatus.Paid)
            {
                return true;
            }

            // 5. Mencegah reaktivasi booking yang sudah dibatalkan manual
            if (booking.Status == BookingStatus.Cancelled)
            {
                if (booking.Payment != null &&
                    (payload.TransactionStatus.Equals("settlement", StringComparison.OrdinalIgnoreCase) ||
                     payload.TransactionStatus.Equals("capture", StringComparison.OrdinalIgnoreCase)))
                {
                    booking.Payment.PaymentStatus = PaymentStatus.Settled;
                    booking.Payment.PaidAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            // 4. Update Status Transaksi Berdasarkan Status dari Midtrans
            if (payload.TransactionStatus.Equals("capture", StringComparison.OrdinalIgnoreCase))
            {
                // Khusus transaksi kartu kredit (capture): pastikan fraud_status adalah "accept"
                if (!string.IsNullOrWhiteSpace(payload.FraudStatus) &&
                    !payload.FraudStatus.Equals("accept", StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Transaksi berstatus challenge/deny, jangan mark Paid
                }

                booking.Status = BookingStatus.Paid;
                if (booking.Payment != null)
                {
                    booking.Payment.PaymentStatus = PaymentStatus.Settled;
                    booking.Payment.PaidAt = DateTime.UtcNow;
                }
            }
            else if (payload.TransactionStatus.Equals("settlement", StringComparison.OrdinalIgnoreCase))
            {
                booking.Status = BookingStatus.Paid;
                if (booking.Payment != null)
                {
                    booking.Payment.PaymentStatus = PaymentStatus.Settled;
                    booking.Payment.PaidAt = DateTime.UtcNow;
                }
            }
            else if (payload.TransactionStatus.Equals("expire", StringComparison.OrdinalIgnoreCase) ||
                     payload.TransactionStatus.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                booking.Status = BookingStatus.Cancelled;
                if (booking.Payment != null)
                {
                    booking.Payment.PaymentStatus = PaymentStatus.Expired;
                }
                // Kembalikan kuota voucher jika transaksi dibatalkan/kedaluwarsa
                if (booking.VoucherId.HasValue)
                {
                    var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == booking.VoucherId.Value);
                    if (voucher != null && voucher.UsedCount > 0)
                    {
                        voucher.UsedCount -= 1;
                        await _cacheService.RemoveAsync($"voucher:{voucher.Code.ToUpper().Trim()}");
                    }
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        private static string ComputeSha512Hash(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = SHA512.HashData(bytes);
            return Convert.ToHexString(hash).ToLower();
        }



    }


}