using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBook.Domain.Entities;

namespace SpotBook.Infrastructure.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.PaymentGatewayRef)
                .HasMaxLength(100);
            builder.Property(p => p.PaymentMethod)
                .HasMaxLength(50);
            builder.Property(p => p.Amount)
                .HasPrecision(18, 2);
            builder.Property(p => p.PaymentStatus)
                .IsRequired()
                .HasConversion<int>();
            // Relasi 1-to-1: 1 Booking memiliki 1 PaymentTransaction
            builder.HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<PaymentTransaction>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }


}