using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Application.DTOs.Request.Report;
using SpotBook.Application.DTOs.Response.Report;

namespace SpotBook.Application.Interfaces.Reports
{
    public interface IReportService
    {
        Task<ReportJobResponse> RequestSalesReportAsync(GenerateReportRequest request);
        Task<ReportJobResponse?> GetReportStatusAsync(Guid jobId);
        Task<(byte[] FileBytes, string FileName)?> DownloadReportAsync(Guid jobId);
    }
}