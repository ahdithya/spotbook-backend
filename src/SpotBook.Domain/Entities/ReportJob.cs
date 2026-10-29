using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotBook.Domain.Enums;

namespace SpotBook.Domain.Entities
{
    public class ReportJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ReportType { get; set; } = "MonthlySalesReport"; // Tipe laporan
        public int Month { get; set; }                                  // Bulan laporan (1-12)
        public int Year { get; set; }                                   // Tahun laporan (misal 2026)
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public string? FilePath { get; set; }                           // Lokasi penyimpanan file CSV fisik
        public string? ErrorMessage { get; set; }                       // Pesan error jika gagal
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}