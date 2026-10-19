using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBook.Domain.Entities;

namespace SpotBook.Infrastructure.Configurations
{
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.ToTable("Vouchers");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Code)
                .IsRequired()
                .HasMaxLength(50);
            // Kode voucher tidak boleh ada yang kembar
            builder.HasIndex(v => v.Code)
                .IsUnique();
            builder.Property(v => v.DiscountType)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(v => v.DiscountValue)
                .HasPrecision(18, 2);
            builder.Property(v => v.MaxDiscountAmount)
                .HasPrecision(18, 2);
            builder.Property(v => v.MinOrderAmount)
                .HasPrecision(18, 2);
        }
    }
}