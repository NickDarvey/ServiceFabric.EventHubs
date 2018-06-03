using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal interface ITestablePartitionReceiver
    {
        Task<IEnumerable<TestableEventData>> ReceiveAsync(int maxMessageCount, TimeSpan waitTime);
        Task CloseAsync();
    }

    internal class TestablePartitionReceiver : ITestablePartitionReceiver
    {
        private readonly PartitionReceiver _receiver;

        public TestablePartitionReceiver(PartitionReceiver receiver) =>
            _receiver = receiver;

        //
        // Summary:
        //     Receive a batch of Microsoft.Azure.EventHubs.EventData's from an EventHub partition
        //     by allowing wait time on each individual call.
        //
        // Returns:
        //     A Task that will yield a batch of Microsoft.Azure.EventHubs.EventData from the
        //     partition on which this receiver is created. Returns 'null' if no EventData is
        //     present.
        public Task<IEnumerable<TestableEventData>> ReceiveAsync(int maxMessageCount, TimeSpan waitTime) =>
            _receiver.ReceiveAsync(maxMessageCount, waitTime)
            .ContinueWith(t => t.Result.Select(e => new TestableEventData(e)));

        //
        // Summary:
        //     Closes and releases resources associated with Microsoft.Azure.EventHubs.PartitionReceiver.
        //
        // Returns:
        //     An asynchronous operation
        public Task CloseAsync() =>
            _receiver.CloseAsync();
    }
}
