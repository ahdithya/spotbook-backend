using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SpotBook.Application.Common.Interfaces;

namespace SpotBook.Infrastructure.services
{
    public class ReportProcessingQueue : IReportProcessingQueue
    {
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

        public ValueTask EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            return _channel.Writer.WriteAsync(jobId, cancellationToken);
        }

        public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
