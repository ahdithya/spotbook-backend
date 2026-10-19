using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Application.DTOs.Request.Voucher;
using SpotBook.Application.DTOs.Response.Voucher;

namespace SpotBook.Application.Interfaces.Vouchers
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherResponse>> GetAllVouchersAsync();
        Task<VoucherResponse> CreateVoucherAsync(CreateVoucherRequest request);
        Task<bool> ToggleVoucherStatusAsync(Guid id);
        Task<VoucherDiscountResult> ApplyVoucherAsync(ApplyVoucherRequest request);
    }
}