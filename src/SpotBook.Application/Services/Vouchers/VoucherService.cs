using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public VoucherService(IAppDbContext context)
        {
            _context = context;
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
                throw new Exception("Kode voucher sudah digunakan.");
            }

            if (request.ValidUntil <= request.ValidFrom)
            {
                throw new Exception("Tanggal berakhir harus lebih besar dari tanggal mulai.");
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

            return new VoucherResponse
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



        }

        public async Task<bool> ToggleVoucherStatusAsync(Guid id)
        {
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null) return false;
            voucher.IsActive = !voucher.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VoucherDiscountResult> ApplyVoucherAsync(ApplyVoucherRequest request)
        {
            var voucher = await _context.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code.ToUpper() == request.Code.ToUpper().Trim());
            if (voucher == null || !voucher.IsActive)
            {
                throw new Exception("Kode voucher tidak valid atau tidak aktif.");
            }
            var now = DateTime.UtcNow;
            if (now < voucher.ValidFrom || now > voucher.ValidUntil)
            {
                throw new Exception("Voucher belum berlaku atau sudah kedaluwarsa.");
            }
            if (voucher.UsedCount >= voucher.Quota)
            {
                throw new Exception("Kuota voucher sudah habis.");
            }
            if (request.OrderAmount < voucher.MinOrderAmount)
            {
                throw new Exception($"Minimal transaksi untuk menggunakan voucher ini adalah Rp{voucher.MinOrderAmount:N0}.");
            }
            // Kalkulasi Potongan Diskon
            decimal discountAmount = 0;
            if (voucher.DiscountType == DiscountType.Percentage)
            {
                discountAmount = request.OrderAmount * (voucher.DiscountValue / 100m);
                // Jika ada batas maksimal potongan diskon persen
                if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
                {
                    discountAmount = voucher.MaxDiscountAmount.Value;
                }
            }
            else if (voucher.DiscountType == DiscountType.FixedAmount)
            {
                discountAmount = voucher.DiscountValue;
            }
            // Diskon tidak boleh melebihi nilai total order    
            if (discountAmount > request.OrderAmount)
            {
                discountAmount = request.OrderAmount;
            }
            decimal finalAmount = request.OrderAmount - discountAmount;

            return new VoucherDiscountResult
            {
                VoucherId = voucher.Id,
                Code = voucher.Code,
                OriginalAmount = request.OrderAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Message = "Voucher berhasil diterapkan!"
            };
        }
    }
}