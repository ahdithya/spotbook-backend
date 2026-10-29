using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotBook.Application.Common.Interfaces;
using SpotBook.Application.Interfaces;
using SpotBook.Domain.Enums;

namespace SpotBook.Infrastructure.services
{
    public class BookingAutoExpiryBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingAutoExpiryBackgroundService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<BookingAutoExpiryBackgroundService> _logger = logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
        private const int ExpiryTimeoutMinutes = 15;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingAutoExpiryBackgroundService aktif berjalan (interval pengecekan: {Interval} detik).", _checkInterval.TotalSeconds);

            using var timer = new PeriodicTimer(_checkInterval);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessExpiredBookingsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan saat memeriksa dan membatalkan reservasi kedaluwarsa.");
                }
            }
        }

        private async Task ProcessExpiredBookingsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            var threshold = DateTime.UtcNow.AddMinutes(-ExpiryTimeoutMinutes);

            var expiredBookings = await context.Bookings
                .Include(b => b.Payment)
                .Include(b => b.Voucher)
                .Where(b => b.Status == BookingStatus.PendingPayment && b.CreatedAt <= threshold)
                .ToListAsync(cancellationToken);

            if (expiredBookings.Count == 0) return;

            foreach (var booking in expiredBookings)
            {
                booking.Status = BookingStatus.Cancelled;

                if (booking.Payment != null)
                {
                    booking.Payment.PaymentStatus = PaymentStatus.Expired;
                }

                if (booking.Voucher != null && booking.Voucher.UsedCount > 0)
                {
                    booking.Voucher.UsedCount -= 1;
                    await cacheService.RemoveAsync($"voucher:{booking.Voucher.Code.ToUpper().Trim()}", cancellationToken);
                }

                _logger.LogInformation("Pemesanan {BookingCode} otomatis dibatalkan karena tidak dibayar dalam {Minutes} menit.", booking.BookingCode, ExpiryTimeoutMinutes);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
