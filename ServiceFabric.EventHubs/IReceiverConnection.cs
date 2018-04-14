using System;
using System.Threading;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public interface IReceiverConnection
    {
        Task RunAsync(
            ProcessEvents processEvents,
            ProcessErrors processErrors,
            int? maxBatchSize = default,
            TimeSpan? waitTime = default,
            CancellationToken cancellationToken = default);
    }
}
