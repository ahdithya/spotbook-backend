using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.Common.Interfaces;
using SpotBook.Application.DTOs.Request.Voucher;
using SpotBook.Application.DTOs.Response.Voucher;
using SpotBook.Application.Interfaces;
using SpotBook.Application.Interfaces.Vouchers;
using SpotBook.Domain.Entities;
using SpotBook.Domain.Enums;

namespace SpotBook.Application.Services.Vouchers
{
    public class VoucherService : IVoucherService
    {
        private readonly IAppDbContext _context;
        private readonly ICacheService _cacheService;


        public VoucherService(IAppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<VoucherResponse>> GetAllVouchersAsync()
        {
            return await _context.Vouchers
            .AsNoTracking()
            .Select(v => new VoucherResponse
            {
                Id = v.Id,
                Code = v.Code,
                DiscountType = v.DiscountType.ToString(),
                DiscountValue = v.DiscountValue,
                MaxDiscountAmount = v.MaxDiscountAmount,
                MinOrderAmount = v.MinOrderAmount,
                Quota = v.Quota,
                UsedCount = v.UsedCount,
                ValidFrom = v.ValidFrom,
                ValidUntil = v.ValidUntil,
                IsActive = v.IsActive
            }).ToListAsync();
        }


        public async Task<VoucherResponse> CreateVoucherAsync(CreateVoucherRequest request)
        {


            var exists = await _context.Vouchers
                        .AnyAsync(v => v.Code.ToUpper() == request.Code.ToUpper());
            if (exists)
            {
                throw new BadRequestException("Kode voucher sudah digunakan.");
            }

            if (request.ValidUntil <= request.ValidFrom)
            {
                throw new BadRequestException("Tanggal berakhir harus lebih besar dari tanggal mulai.");
            }

            var voucher = new Voucher
            {
                Code = request.Code.ToUpper().Trim(),
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                MinOrderAmount = request.MinOrderAmount,
                Quota = request.Quota,
                UsedCount = 0,
                ValidFrom = DateTime.SpecifyKind(request.ValidFrom, DateTimeKind.Utc),
                ValidUntil = DateTime.SpecifyKind(request.ValidUntil, DateTimeKind.Utc),
                IsActive = true
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            var response = new VoucherResponse
            {
                Id = voucher.Id,
                Code = voucher.Code,
                DiscountType = voucher.DiscountType.ToString(),
                DiscountValue = voucher.DiscountValue,
                MaxDiscountAmount = voucher.MaxDiscountAmount,
                MinOrderAmount = voucher.MinOrderAmount,
                Quota = voucher.Quota,
                UsedCount = voucher.UsedCount,
                ValidFrom = voucher.ValidFrom,
                ValidUntil = voucher.ValidUntil,
                IsActive = voucher.IsActive
            };


            var cacheKey = $"voucher:{request.Code.ToUpper().Trim()}";
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));
            return response;


        }

        public async Task<bool> ToggleVoucherStatusAsync(Guid id)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null) return false;
            voucher.IsActive = !voucher.IsActive;
            await _context.SaveChangesAsync();


            var cacheKey = $"voucher:{voucher.Code}";
            await _cacheService.RemoveAsync(cacheKey);
            return true;
        }
        public async Task<VoucherDiscountResult> ApplyVoucherAsync(ApplyVoucherRequest request)
        {
            var codeClean = request.Code.ToUpper().Trim();
            var cacheKey = $"voucher:{codeClean}";

            // 1. Cek di Redis dulu (manfaatkan data yang sudah kamu set saat CreateVoucher!)
            var cached = await _cacheService.GetAsync<VoucherResponse>(cacheKey);

            // Variabel untuk kalkulasi
            bool isActive;
            DateTime validFrom, validUntil;
            int usedCount, quota;
            decimal minOrderAmount, discountValue;
            decimal? maxDiscountAmount;
            string discountType;
            Guid voucherId;
            string voucherCode;

            if (cached != null)
            {
                // AMBIL DARI REDIS (Super cepat < 1ms)
                voucherId = cached.Id;
                voucherCode = cached.Code;
                isActive = cached.IsActive;
                validFrom = cached.ValidFrom;
                validUntil = cached.ValidUntil;
                usedCount = cached.UsedCount;
                quota = cached.Quota;
                minOrderAmount = cached.MinOrderAmount;
                discountType = cached.DiscountType;
                discountValue = cached.DiscountValue;
                maxDiscountAmount = cached.MaxDiscountAmount;
            }
            else
            {
                // JIKA CACHE MISS, AMBIL DARI POSTGRESQL
                var dbVoucher = await _context.Vouchers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Code.ToUpper() == codeClean);

                if (dbVoucher == null)
                {
                    throw new NotFoundException("Kode voucher tidak valid.");
                }

                voucherId = dbVoucher.Id;
                voucherCode = dbVoucher.Code;
                isActive = dbVoucher.IsActive;
                validFrom = dbVoucher.ValidFrom;
                validUntil = dbVoucher.ValidUntil;
                usedCount = dbVoucher.UsedCount;
                quota = dbVoucher.Quota;
                minOrderAmount = dbVoucher.MinOrderAmount;
                discountType = dbVoucher.DiscountType.ToString();
                discountValue = dbVoucher.DiscountValue;
                maxDiscountAmount = dbVoucher.MaxDiscountAmount;

                // Simpan ke Redis untuk request customer berikutnya
                var toCache = new VoucherResponse
                {
                    Id = dbVoucher.Id,
                    Code = dbVoucher.Code,
                    DiscountType = discountType,
                    DiscountValue = discountValue,
                    MaxDiscountAmount = maxDiscountAmount,
                    MinOrderAmount = minOrderAmount,
                    Quota = quota,
                    UsedCount = usedCount,
                    ValidFrom = validFrom,
                    ValidUntil = validUntil,
                    IsActive = isActive
                };
                await _cacheService.SetAsync(cacheKey, toCache, TimeSpan.FromMinutes(15));
            }

            // 2. Lakukan validasi bisnis & hitung potongan harga seperti biasa
            if (!isActive) throw new BadRequestException("Kode voucher tidak aktif.");
            var now = DateTime.UtcNow;
            if (now < validFrom || now > validUntil) throw new BadRequestException("Voucher belum berlaku atau sudah kedaluwarsa.");
            if (usedCount >= quota) throw new BadRequestException("Kuota voucher sudah habis.");
            if (request.OrderAmount < minOrderAmount) throw new BadRequestException($"Minimal transaksi adalah Rp{minOrderAmount:N0}.");

            decimal discountAmount = 0;
            if (discountType == DiscountType.Percentage.ToString())
            {
                discountAmount = request.OrderAmount * (discountValue / 100m);
                if (maxDiscountAmount.HasValue && discountAmount > maxDiscountAmount.Value)
                    discountAmount = maxDiscountAmount.Value;
            }
            else
            {
                discountAmount = discountValue;
            }

            if (discountAmount > request.OrderAmount) discountAmount = request.OrderAmount;
            decimal finalAmount = request.OrderAmount - discountAmount;
            if (finalAmount <= 0)
            {
                finalAmount = 1m;
                discountAmount = Math.Max(0, request.OrderAmount - finalAmount);
            }

            return new VoucherDiscountResult
            {
                VoucherId = voucherId,
                Code = voucherCode,
                OriginalAmount = request.OrderAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Message = "Voucher berhasil diterapkan!"
            };
        }
    }
}