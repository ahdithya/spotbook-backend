using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.DTOs.Request.Voucher;
using SpotBook.Application.DTOs.Response.Voucher;
using SpotBook.Application.Interfaces.Vouchers;

namespace SpotBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VoucherResponse>>> GetAll()
        {
            var result = await _voucherService.GetAllVouchersAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<VoucherResponse>> Create([FromBody] CreateVoucherRequest request)
        {
            try
            {
                var result = await _voucherService.CreateVoucherAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var success = await _voucherService.ToggleVoucherStatusAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Voucher tidak ditemukan." });
            }
            return Ok(new { message = "Status voucher berhasil diperbarui." });
        }

        [Authorize]
        [HttpPost("apply")]
        public async Task<ActionResult<VoucherDiscountResult>> Apply([FromBody] ApplyVoucherRequest request)
        {
            try
            {
                var result = await _voucherService.ApplyVoucherAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }



}