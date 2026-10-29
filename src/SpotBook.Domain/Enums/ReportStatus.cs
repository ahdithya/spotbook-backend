namespace SpotBook.Domain.Enums
{
    public enum ReportStatus
    {
        Pending = 1,     // Baru dimasukkan ke antrean
        Processing = 2,  // Sedang diproses oleh Background Worker
        Completed = 3,   // Selesai dibuat, file siap diunduh
        Failed = 4       // Gagal (misal error I/O atau memori)
    }
}