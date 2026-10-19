using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBook.Domain.Entities;

namespace SpotBook.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);
            // Email harus unik di database agar tidak ada 2 akun dengan email sama
            builder.HasIndex(u => u.Email)
                .IsUnique();
            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);
            builder.Property(u => u.PasswordHash)
                .IsRequired();
            // Menyimpan enum UserRole sebagai string atau integer (integer lebih cepat & hemat memori)
            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<int>();
            builder.Property(u => u.RefreshToken)
                .HasMaxLength(255);
        }
    }
}