using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBook.Domain.Entities;

namespace SpotBook.Infrastructure.Configurations
{
    public class SpaceConfiguration : IEntityTypeConfiguration<Space>
    {
        public void Configure(EntityTypeBuilder<Space> builder)
        {
            builder.ToTable("Spaces");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(s => s.Description)
                .HasMaxLength(500);
            // Presisi angka desimal untuk mata uang di PostgreSQL: decimal(18,2)
            builder.Property(s => s.PricePerHour)
                .HasPrecision(18, 2);
        }
    }
}