using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SpotBook.Domain.Entities;

namespace SpotBook.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Space> Spaces { get; }
        DbSet<Booking> Bookings { get; }
        DbSet<Voucher> Vouchers { get; }
        DbSet<PaymentTransaction> PaymentTransactions { get; }
        DbSet<ReportJob> ReportJobs { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}