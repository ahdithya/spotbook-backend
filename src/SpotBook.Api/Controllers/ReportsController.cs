using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.Common.Models;
using SpotBook.Application.DTOs.Request.Report;
using SpotBook.Application.DTOs.Response.Report;
using SpotBook.Application.Interfaces.Reports;

namespace SpotBook.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController(IReportService reportService) : BaseApiController
    {
        private readonly IReportService _reportService = reportService;

        [HttpPost("sales")]
        public async Task<ActionResult<ApiResponse<ReportJobResponse>>> RequestSalesReport([FromBody] GenerateReportRequest request)
        {
            var result = await _reportService.RequestSalesReportAsync(request);
            return HandleSuccess(result, "Permintaan pembuatan laporan berhasil diajukan.");
        }

        [HttpGet("{id:guid}/status")]
        public async Task<ActionResult<ApiResponse<ReportJobResponse>>> GetStatus(Guid id)
        {
            var result = await _reportService.GetReportStatusAsync(id)
                ?? throw new NotFoundException("Job laporan tidak ditemukan.");

            return HandleSuccess(result, "Status laporan berhasil diambil.");
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var result = await _reportService.DownloadReportAsync(id);
            if (result == null)
            {
                throw new NotFoundException("Job laporan tidak ditemukan.");
            }

            return File(result.Value.FileBytes, "text/csv", result.Value.FileName);
        }
    }
}
