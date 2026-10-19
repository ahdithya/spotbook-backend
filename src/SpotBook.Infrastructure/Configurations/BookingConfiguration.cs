using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBook.Domain.Entities;

namespace SpotBook.Infrastructure.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.BookingCode)
                .IsRequired()
                .HasMaxLength(30);
            builder.HasIndex(b => b.BookingCode)
                .IsUnique();
            builder.Property(b => b.BasePrice).HasPrecision(18, 2);
            builder.Property(b => b.DiscountAmount).HasPrecision(18, 2);
            builder.Property(b => b.TotalAmount).HasPrecision(18, 2);
            builder.Property(b => b.Status)
                .IsRequired()
                .HasConversion<int>();
            // Relasi ke User (1 User memiliki banyak Booking)
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Mencegah data booking ikut terhapus jika akun user dihapus
                                                    // Relasi ke Space (1 Space memiliki banyak Booking)
            builder.HasOne(b => b.Space)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SpaceId)
                .OnDelete(DeleteBehavior.Restrict);
            // Relasi ke Voucher (Opsional)
            builder.HasOne(b => b.Voucher)
                .WithMany()
                .HasForeignKey(b => b.VoucherId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            // Indeks gabungan untuk mempercepat validasi bentrok jadwal (Overlap Query)
            builder.HasIndex(b => new { b.SpaceId, b.StartTime, b.EndTime });
        }
    }
}