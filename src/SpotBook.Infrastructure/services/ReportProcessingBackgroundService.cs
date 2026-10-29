using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotBook.Application.Common.Interfaces;
using SpotBook.Application.Interfaces;
using SpotBook.Domain.Enums;

namespace SpotBook.Infrastructure.services
{
    public class ReportProcessingBackgroundService(
        IReportProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ReportProcessingBackgroundService> logger) : BackgroundService
    {
        private readonly IReportProcessingQueue _queue = queue;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<ReportProcessingBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReportProcessingBackgroundService aktif mendengarkan antrean pekerjaan.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var jobId = await _queue.DequeueAsync(stoppingToken);
                    await ProcessJobAsync(jobId, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Terjadi kesalahan saat memproses antrean job laporan.");
                }
            }
        }

        private async Task ProcessJobAsync(Guid jobId, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var job = await context.ReportJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
            if (job == null) return;

            try
            {
                job.Status = ReportStatus.Processing;
                await context.SaveChangesAsync(cancellationToken);

                var startDate = new DateTime(job.Year, job.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);

                var bookings = await context.Bookings
                    .AsNoTracking()
                    .Include(b => b.User)
                    .Include(b => b.Space)
                    .Where(b => b.StartTime >= startDate && b.StartTime < endDate &&
                                (b.Status == BookingStatus.Paid || b.Status == BookingStatus.Completed))
                    .OrderBy(b => b.StartTime)
                    .ToListAsync(cancellationToken);

                var sb = new StringBuilder();
                sb.AppendLine("BookingCode,CustomerName,CustomerEmail,SpaceName,StartTime,EndTime,DurationHours,BasePrice,DiscountAmount,TotalAmount,Status,CreatedAt");

                foreach (var b in bookings)
                {
                    var duration = (decimal)(b.EndTime - b.StartTime).TotalHours;
                    var custName = b.User?.FullName?.Replace("\"", "\"\"") ?? "-";
                    var custEmail = b.User?.Email?.Replace("\"", "\"\"") ?? "-";
                    var spaceName = b.Space?.Name?.Replace("\"", "\"\"") ?? "-";

                    sb.AppendLine($"\"{b.BookingCode}\",\"{custName}\",\"{custEmail}\",\"{spaceName}\",\"{b.StartTime:yyyy-MM-dd HH:mm}\",\"{b.EndTime:yyyy-MM-dd HH:mm}\",{duration:F2},{b.BasePrice},{b.DiscountAmount},{b.TotalAmount},\"{b.Status}\",\"{b.CreatedAt:yyyy-MM-dd HH:mm}\"");
                }

                var storageDir = Path.Combine(AppContext.BaseDirectory, "storage", "reports");
                Directory.CreateDirectory(storageDir);

                var filePath = Path.Combine(storageDir, $"{job.Id}.csv");
                await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, cancellationToken);

                job.Status = ReportStatus.Completed;
                job.FilePath = filePath;
                job.CompletedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Job laporan {JobId} selesai dibuat: {FilePath}", job.Id, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memproses job laporan {JobId}", jobId);
                job.Status = ReportStatus.Failed;
                job.ErrorMessage = ex.Message;
                await context.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
