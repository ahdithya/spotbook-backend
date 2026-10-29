using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBook.Domain.Entities;

namespace SpotBook.Infrastructure.Configurations
{
    public class ReportJobConfiguration : IEntityTypeConfiguration<ReportJob>
    {
        public void Configure(EntityTypeBuilder<ReportJob> builder)
        {
            builder.ToTable("ReportJobs");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.ReportType)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(r => r.FilePath)
                .HasMaxLength(500);
            builder.Property(r => r.ErrorMessage)
                .HasMaxLength(1000);
        }
    }
}