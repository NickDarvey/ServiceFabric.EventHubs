using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using NickDarvey.ServiceFabric.EventHubs.Schema;
using static NickDarvey.ServiceFabric.EventHubs.States;

namespace NickDarvey.ServiceFabric.EventHubs
{
    class ReliableEventHubReceiverConnectionFactory : IReceiverConnectionFactory
    {
        private readonly ITestableEventHubClient _client;
        private readonly IReliableStateManager _state;
        private readonly Func<SaveCheckpoint, CreateHandler> _handlers;
        private readonly Func<long, Task<string>> _partitions;

        public ReliableEventHubReceiverConnectionFactory(
            ITestableEventHubClient client,
            IReliableStateManager state,
            Func<SaveCheckpoint, CreateHandler> handlers,
            Func<long, Task<string>> partitions,
            EventPosition initialPosition = default,
            uint? initialEpoch = default)
        {
            _client = client;
            _state = state;
            _handlers = handlers;
            _partitions = partitions;
            InitialPosition = initialPosition ?? InitialPosition;
            InitialEpoch = initialEpoch ?? InitialEpoch;
        }

        public EventPosition InitialPosition { get; internal set; } = EventPosition.FromEnd();
        public long InitialEpoch { get; internal set; } = 0;

        public async Task<IReceiverConnection> CreateReceiver(
            long partitionKey,
            string consumerGroupName,
            CancellationToken cancel = default)
        {
            var partitionId = await _partitions(partitionKey);
            return await TakeLease(consumerGroupName, partitionId);
        }

        private async Task<IReceiverConnection> TakeLease(string consumerGroupName, string partitionId)
        {
            var checkpoints = await GetCheckpointState(_state, _client.EventHubName, consumerGroupName);
            var leases = await GetLeaseState(_state, _client.EventHubName, consumerGroupName);
            async Task Checkpointer(Checkpoint checkpoint)
            {
                using (var tx = _state.CreateTransaction())
                {
                    await checkpoints.SetAsync(tx, partitionId, checkpoint);
                    await tx.CommitAsync();
                }
            }
            var handlers = _handlers(Checkpointer);

            using (var tx = _state.CreateTransaction())
            {
                var checkpointState = await checkpoints.TryGetValueAsync(tx, partitionId, LockMode.Update);
                var leaseState = await leases.TryGetValueAsync(tx, partitionId, LockMode.Update);

                var position = checkpointState.HasValue
                    ? EventPosition.FromOffset(checkpointState.Value.Offset)
                    : InitialPosition;

                var epoch = leaseState.HasValue
                    ? leaseState.Value.Epoch
                    : InitialEpoch;

                var receiver = _client.CreateEpochReceiver(consumerGroupName, partitionId, position, epoch);

                await leases.SetAsync(tx, partitionId, new Lease(epoch));

                await tx.CommitAsync();

                return new LoopingEventHubReceiverConnection(receiver, handlers, Loops.Infinite());
            }
        }
    }
}
