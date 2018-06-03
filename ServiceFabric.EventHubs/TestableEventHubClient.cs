using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal interface ITestableEventHubClient
    {
        string EventHubName { get; }
        ITestablePartitionReceiver CreateEpochReceiver(string consumerGroupName, string partitionId, EventPosition eventPosition, long epoch, ReceiverOptions receiverOptions = null);
        Task<EventHubRuntimeInformation> GetRuntimeInformationAsync();
    }

    internal class TestableEventHubClient : ITestableEventHubClient
    {
        private readonly EventHubClient _client;

        public TestableEventHubClient(EventHubClient client) =>
            _client = client;

        //
        // Summary:
        //     Gets the name of the EventHub.
        public string EventHubName { get => _client.EventHubName; }

        //
        // Summary:
        //     Create a Epoch based EventHub receiver with given Microsoft.Azure.EventHubs.EventPosition.
        //     The receiver is created for a specific EventHub Partition from the specific consumer
        //     group.
        //     It is important to pay attention to the following when creating epoch based receiver:
        //     - Ownership enforcement: Once you created an epoch based receiver, you cannot
        //     create a non-epoch receiver to the same consumerGroup-Partition combo until all
        //     receivers to the combo are closed.
        //     - Ownership stealing: If a receiver with higher epoch value is created for a
        //     consumerGroup-Partition combo, any older epoch receiver to that combo will be
        //     force closed.
        //     - Any receiver closed due to lost of ownership to a consumerGroup-Partition combo
        //     will get ReceiverDisconnectedException for all operations from that receiver.
        //
        // Parameters:
        //   consumerGroupName:
        //     the consumer group name that this receiver should be grouped under.
        //
        //   partitionId:
        //     the partition Id that the receiver belongs to. All data received will be from
        //     this partition only.
        //
        //   eventPosition:
        //     The starting Microsoft.Azure.EventHubs.EventPosition at which to start receiving
        //     messages.
        //
        //   epoch:
        //     a unique identifier (epoch value) that the service uses, to enforce partition/lease
        //     ownership.
        //
        //   receiverOptions:
        //     Options for a event hub receiver.
        //
        // Returns:
        //     The created PartitionReceiver
        public ITestablePartitionReceiver CreateEpochReceiver(string consumerGroupName, string partitionId, EventPosition eventPosition, long epoch, ReceiverOptions receiverOptions = null) =>
            new TestablePartitionReceiver(_client.CreateEpochReceiver(consumerGroupName, partitionId, eventPosition, epoch, receiverOptions));

        //
        // Summary:
        //     Retrieves EventHub runtime information
        public Task<EventHubRuntimeInformation> GetRuntimeInformationAsync() =>
            _client.GetRuntimeInformationAsync();
    }
}
