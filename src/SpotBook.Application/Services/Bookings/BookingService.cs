using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotBook.Application.Common.Interfaces;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.DTOs.Request.Booking;
using SpotBook.Application.DTOs.Response.Booking;
using SpotBook.Application.Interfaces;
using SpotBook.Application.Interfaces.Bookings;
using SpotBook.Domain.Entities;
using SpotBook.Domain.Enums;

namespace SpotBook.Application.Services.Bookings
{
    public class BookingService(IAppDbContext context, ICacheService cacheService) : IBookingService
    {
        private readonly IAppDbContext _context = context;
        private readonly ICacheService _cacheService = cacheService;

        public async Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request)
        {
            // validasi dan normalisasi waktu ke UTC
            var startTimeUtc = NormalizeToUtc(request.StartTime);
            var endTimeUtc = NormalizeToUtc(request.EndTime);

            if (startTimeUtc <= DateTime.UtcNow)
            {
                throw new BadRequestException("Waktu sewa tidak boleh di masa lalu.");
            }
            if (endTimeUtc <= startTimeUtc)
            {
                throw new BadRequestException("Waktu selesai harus lebih besar dari waktu mulai.");
            }

            var duration = (decimal)(endTimeUtc - startTimeUtc).TotalHours;
            if (duration < 1)
            {
                throw new BadRequestException("Minimal durasi pemesanan adalah 1 jam.");
            }

            using var transaction = await _context.BeginTransactionAsync();
            try
            {
                var space = await _context.Spaces
                    .FromSqlInterpolated($"SELECT * FROM \"Spaces\" WHERE \"Id\" = {request.SpaceId} FOR UPDATE")
                    .FirstOrDefaultAsync();

                if (space == null || !space.IsActive)
                {
                    throw new NotFoundException("Ruangan tidak ditemukan atau sedang tidak aktif.");
                }

                // Rumus: StartA < EndB && EndA > StartB
                // Abaikan booking yang statusnya sudah "Cancelled"
                var isConflict = await _context.Bookings
                                .AnyAsync(b => b.SpaceId == request.SpaceId
                                          && b.Status != BookingStatus.Cancelled
                                          && b.StartTime < endTimeUtc
                                          && b.EndTime > startTimeUtc);

                if (isConflict)
                {
                    throw new BadRequestException("Jadwal pada ruangan ini sudah dipesan orang lain. Silakan pilih jam atau ruangan lain.");
                }

                // Kalkulasi Harga Dasar(Base Price)
                decimal basePrice = Math.Round(duration * space.PricePerHour, 2);
                decimal discountAmount = 0;
                Guid? voucherId = null;

                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    var cleanCode = request.VoucherCode.ToUpper().Trim();
                    var voucher = await _context.Vouchers
                        .FromSqlInterpolated($"SELECT * FROM \"Vouchers\" WHERE UPPER(\"Code\") = {cleanCode} FOR UPDATE")
                        .FirstOrDefaultAsync();

                    if (voucher == null || !voucher.IsActive)
                    {
                        throw new BadRequestException("Kode voucher tidak valid atau tidak aktif.");
                    }
                    var now = DateTime.UtcNow;
                    if (now < voucher.ValidFrom || now > voucher.ValidUntil)
                    {
                        throw new BadRequestException("Voucher belum berlaku atau sudah kedaluwarsa.");
                    }
                    if (voucher.UsedCount >= voucher.Quota)
                    {
                        throw new BadRequestException("Kuota voucher sudah habis.");
                    }
                    if (basePrice < voucher.MinOrderAmount)
                    {
                        throw new BadRequestException($"Minimal transaksi untuk voucher ini adalah Rp{voucher.MinOrderAmount:N0}.");
                    }
                    if (voucher.DiscountType == DiscountType.Percentage)
                    {
                        discountAmount = basePrice * (voucher.DiscountValue / 100m);
                        if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
                        {
                            discountAmount = voucher.MaxDiscountAmount.Value;
                        }

                    }
                    else if (voucher.DiscountType == DiscountType.FixedAmount)
                    {
                        discountAmount = voucher.DiscountValue;
                    }

                    if (discountAmount > basePrice)
                    {
                        discountAmount = basePrice;
                    }
                    // Tambahkan hitungan pemakaian kuota voucher
                    voucher.UsedCount += 1;
                    voucherId = voucher.Id;
                }


                decimal totalAmount = basePrice - discountAmount;
                if (totalAmount <= 0)
                {
                    totalAmount = 1m; // Minimal Rp 1 agar valid diproses payment gateway Midtrans
                    discountAmount = Math.Max(0, basePrice - totalAmount);
                }
                string bookingCode = $"SB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    UserId = userId,
                    SpaceId = space.Id,
                    StartTime = startTimeUtc,
                    EndTime = endTimeUtc,
                    BasePrice = basePrice,
                    VoucherId = voucherId,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    Status = BookingStatus.PendingPayment,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                // 8. Commit Transaksi Database jika semua operasi di atas sukses tanpa error
                await transaction.CommitAsync();

                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    await _cacheService.RemoveAsync($"voucher:{request.VoucherCode.ToUpper().Trim()}");
                }


                return new BookingResponse
                {
                    Id = booking.Id,
                    BookingCode = booking.BookingCode,
                    SpaceId = space.Id,
                    SpaceName = space.Name,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    DurationInHours = duration,
                    BasePrice = booking.BasePrice,
                    DiscountAmount = booking.DiscountAmount,
                    TotalAmount = booking.TotalAmount,
                    Status = booking.Status.ToString(),
                    CreatedAt = booking.CreatedAt
                };



            }
            catch
            {      // Jika ada kesalahan atau bentrok di tengah jalan, batalkan semua perubahan!
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<BookingResponse>> GetUserBookingsAsync(Guid userId)
        {
            return await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Space)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    BookingCode = b.BookingCode,
                    SpaceId = b.SpaceId,
                    SpaceName = b.Space.Name,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    DurationInHours = (decimal)(b.EndTime - b.StartTime).TotalHours,
                    BasePrice = b.BasePrice,
                    DiscountAmount = b.DiscountAmount,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                }).ToListAsync();
        }


        public async Task<BookingResponse?> GetBookingByIdAsync(Guid bookingId, Guid userId)
        {
            var booking = await _context.Bookings
                        .AsNoTracking()
                        .Include(b => b.Space)
                        .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);


            if (booking == null)
            {
                return null;
            }
            return new BookingResponse
            {
                Id = booking.Id,
                BookingCode = booking.BookingCode,
                SpaceId = booking.SpaceId,
                SpaceName = booking.Space.Name,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                DurationInHours = (decimal)(booking.EndTime - booking.StartTime).TotalHours,
                BasePrice = booking.BasePrice,
                DiscountAmount = booking.DiscountAmount,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt
            };
        }


        public async Task<bool> CancelBookingAsync(Guid bookingId, Guid userId)
        {

            var booking = await _context.Bookings
                            .Include(b => b.Voucher)
                            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
            if (booking == null) return false;

            if (booking.Status != BookingStatus.PendingPayment)
            {
                throw new BadRequestException("Hanya pemesanan yang menunggu pembayaran yang dapat dibatalkan.");
            }



            booking.Status = BookingStatus.Cancelled;
            // Jika booking ini sempat memakai voucher, kembalikan kuotanya!
            if (booking.Voucher != null && booking.Voucher.UsedCount > 0)
            {
                booking.Voucher.UsedCount -= 1;
                await _cacheService.RemoveAsync($"voucher:{booking.Voucher.Code.ToUpper().Trim()}");
            }
            await _context.SaveChangesAsync();
            return true;
        }





        private static DateTime NormalizeToUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }
    }
}