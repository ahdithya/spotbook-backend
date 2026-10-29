using System;
using System.Threading;
using System.Threading.Tasks;

namespace SpotBook.Application.Common.Interfaces
{
    public interface IReportProcessingQueue
    {
        ValueTask EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default);
        ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken = default);
    }
}
