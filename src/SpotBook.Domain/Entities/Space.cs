using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotBook.Domain.Entities
{
    public class Space
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;           // Contoh: "Meeting Room Alpha"
        public string Description { get; set; } = string.Empty;    // Deskripsi ruangan & fasilitas
        public int Capacity { get; set; }                          // Kapasitas orang (misal: 10)
        public decimal PricePerHour { get; set; }                  // Harga sewa per jam
        public bool IsActive { get; set; } = true;                 // Apakah ruangan sedang dibuka/tutup
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Relasi navigasi: 1 Space bisa memiliki banyak Booking
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
