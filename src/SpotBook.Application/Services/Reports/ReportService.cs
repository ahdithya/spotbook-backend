using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.Common.Interfaces;
using SpotBook.Application.DTOs.Request.Report;
using SpotBook.Application.DTOs.Response.Report;
using SpotBook.Application.Interfaces;
using SpotBook.Application.Interfaces.Reports;
using SpotBook.Domain.Entities;
using SpotBook.Domain.Enums;

namespace SpotBook.Application.Services.Reports
{
    public class ReportService(IAppDbContext context, IReportProcessingQueue queue) : IReportService
    {
        private readonly IAppDbContext _context = context;
        private readonly IReportProcessingQueue _queue = queue;

        public async Task<ReportJobResponse> RequestSalesReportAsync(GenerateReportRequest request)
        {
            if (request.Month < 1 || request.Month > 12)
            {
                throw new BadRequestException("Bulan laporan harus berada pada rentang 1 sampai 12.");
            }

            if (request.Year < 2020 || request.Year > 2100)
            {
                throw new BadRequestException("Tahun laporan tidak valid.");
            }

            var job = new ReportJob
            {
                ReportType = "MonthlySalesReport",
                Month = request.Month,
                Year = request.Year,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.ReportJobs.Add(job);
            await _context.SaveChangesAsync();

            await _queue.EnqueueAsync(job.Id);

            return new ReportJobResponse
            {
                JobId = job.Id,
                ReportType = job.ReportType,
                Month = job.Month,
                Year = job.Year,
                Status = job.Status.ToString(),
                CreatedAt = job.CreatedAt,
                CompletedAt = job.CompletedAt,
                Message = "Permintaan laporan berhasil didaftarkan dan sedang dalam antrean pemrosesan."
            };
        }

        public async Task<ReportJobResponse?> GetReportStatusAsync(Guid jobId)
        {
            var job = await _context.ReportJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null) return null;

            string message = job.Status switch
            {
                ReportStatus.Pending => "Laporan sedang dalam antrean.",
                ReportStatus.Processing => "Laporan sedang diproses dan dihitung.",
                ReportStatus.Completed => "Laporan selesai dibuat dan siap diunduh.",
                ReportStatus.Failed => $"Pembuatan laporan gagal: {job.ErrorMessage ?? "Terjadi kesalahan internal"}",
                _ => "Status tidak diketahui."
            };

            return new ReportJobResponse
            {
                JobId = job.Id,
                ReportType = job.ReportType,
                Month = job.Month,
                Year = job.Year,
                Status = job.Status.ToString(),
                CreatedAt = job.CreatedAt,
                CompletedAt = job.CompletedAt,
                Message = message
            };
        }

        public async Task<(byte[] FileBytes, string FileName)?> DownloadReportAsync(Guid jobId)
        {
            var job = await _context.ReportJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null) return null;

            if (job.Status != ReportStatus.Completed)
            {
                throw new BadRequestException($"Laporan belum selesai dibuat (Status saat ini: {job.Status}).");
            }

            if (string.IsNullOrWhiteSpace(job.FilePath) || !File.Exists(job.FilePath))
            {
                throw new NotFoundException("File laporan tidak ditemukan pada penyimpanan server.");
            }

            var fileBytes = await File.ReadAllBytesAsync(job.FilePath);
            var fileName = $"SalesReport_{job.Year}_{job.Month:D2}.csv";
            return (fileBytes, fileName);
        }
    }
}
