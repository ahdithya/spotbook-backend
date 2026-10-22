using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.Common.Models;
using SpotBook.Application.DTOs.Request.Voucher;
using SpotBook.Application.DTOs.Response.Voucher;
using SpotBook.Application.Interfaces.Vouchers;

namespace SpotBook.Api.Controllers;

public class VouchersController(IVoucherService voucherService) : BaseApiController
{
    private readonly IVoucherService _voucherService = voucherService;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<VoucherResponse>>>> GetAll()
    {
        var result = await _voucherService.GetAllVouchersAsync();
        return HandleSuccess(result, "Daftar voucher berhasil diambil.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VoucherResponse>>> Create([FromBody] CreateVoucherRequest request)
    {
        var result = await _voucherService.CreateVoucherAsync(request);
        return HandleSuccess(result, "Voucher berhasil dibuat.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleStatus(Guid id)
    {
        var success = await _voucherService.ToggleVoucherStatusAsync(id);
        if (!success)
        {
            throw new NotFoundException("Voucher tidak ditemukan.");
        }
        return HandleSuccess("Status voucher berhasil diperbarui.");
    }

    [Authorize]
    [HttpPost("apply")]
    public async Task<ActionResult<ApiResponse<VoucherDiscountResult>>> Apply([FromBody] ApplyVoucherRequest request)
    {
        var result = await _voucherService.ApplyVoucherAsync(request);
        return HandleSuccess(result, "Voucher berhasil diterapkan.");
    }
}
