using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Response.Report
{
    public class ReportJobResponse
    {
        public Guid JobId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}