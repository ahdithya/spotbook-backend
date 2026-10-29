using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Application.DTOs.Request.Report
{
    public class GenerateReportRequest
    {
        [Range(1, 12, ErrorMessage = "Bulan harus antara 1 sampai 12.")]
        public int Month { get; set; } = DateTime.UtcNow.Month;

        [Range(2020, 2100, ErrorMessage = "Tahun tidak valid.")]
        public int Year { get; set; } = DateTime.UtcNow.Year;
    }
}