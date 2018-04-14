using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using static NickDarvey.ServiceFabric.EventHubs.Diagnostics;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal class LoopingEventHubReceiverConnection : IReceiverConnection
    {
        private readonly PartitionReceiver _receiver;
        private readonly CreateHandler _handlers;
        private readonly IEnumerable<Unit> _loop;

        public LoopingEventHubReceiverConnection(
            PartitionReceiver receiver,
            CreateHandler handlers,
            IEnumerable<Unit> loop)
        {
            _receiver = receiver;
            _handlers = handlers;
            _loop = loop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// We can't use PartitionReceiver.SetReceiveHandler because it has an Environment.FailFast()
        /// which might kill multiple partitions unnecessarily if they're hosted in the same process in Service Fabric.
        /// </remarks>
        /// <param name="processor"></param>
        /// <param name="maxBatchSize"></param>
        /// <param name="waitTime"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RunAsync(
            ProcessEvents processEvents,
            ProcessErrors processErrors,
            int? maxBatchSize = default,
            TimeSpan? waitTime = default,
            CancellationToken cancellationToken = default)
        {
            var handler = _handlers(processEvents, processErrors);

            foreach (var _ in _loop)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var activity = new Activity("Process");

                try
                {
                    if (ReceiveDiagnostics.IsEnabled("Process"))
                        ReceiveDiagnostics.StartActivity(
                        activity: activity,
                        args: new { });

                    var events = await _receiver.ReceiveAsync(
                        maxMessageCount: maxBatchSize ?? Constants.DefaultMaxBatchSize,
                        waitTime: waitTime ?? Constants.DefaultOperationTimeout).ConfigureAwait(false);

                    if (events == null) continue;

                    await handler.ProcessEventsAsync(events).ConfigureAwait(false);
                }
                catch (ReceiverDisconnectedException ex)
                {
                    if (ReceiveDiagnostics.IsEnabled("Disconnection"))
                        ReceiveDiagnostics.Write("Disconnection", new { Exception = ex });

                    // Another partition has picked up the work,
                    // end the loop and finish RunAsync.
                    await _receiver.CloseAsync().ConfigureAwait(false);
                    break;
                }
                catch (Exception ex)
                {
                    if (ReceiveDiagnostics.IsEnabled("Failure"))
                        ReceiveDiagnostics.Write("Failure", new { Exception = ex });

                    await handler.ProcessErrorAsync(ex).ConfigureAwait(false);
                    throw;
                }
                finally
                {
                    if (ReceiveDiagnostics.IsEnabled("Process"))
                        ReceiveDiagnostics.StopActivity(
                        activity: activity,
                        args: new { });
                }
            }
        }
    }
}
