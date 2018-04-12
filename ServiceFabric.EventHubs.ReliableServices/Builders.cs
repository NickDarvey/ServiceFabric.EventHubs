using Microsoft.Azure.EventHubs;
using Microsoft.ServiceFabric.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public static class Builders
    {
        public static IReceiverConnectionFactory WithServiceFabricIntegration(this EventHubClient client, IReliableStateManager state) =>
            client.UseServiceFabricState(state).WithBatchCheckpointing();

        public struct ServiceFabricEventHubsBuilder
        {
            internal EventHubClient Client { get; }
            internal IReliableStateManager StateManager { get; }

            internal ServiceFabricEventHubsBuilder(EventHubClient client, IReliableStateManager stateManager)
            {
                Client = client;
                StateManager = stateManager;
            }
        }

        public static ServiceFabricEventHubsBuilder UseServiceFabricState(this EventHubClient client, IReliableStateManager state) =>
            new ServiceFabricEventHubsBuilder(client, state);

        public static IReceiverConnectionFactory WithBatchCheckpointing(this ServiceFabricEventHubsBuilder builder) =>
            new ReliableEventHubReceiverConnectionFactory(builder.Client, builder.StateManager, checkpointer => (events, errors) => new BatchCheckpointEventHandler(events, errors, checkpointer));

        public static IReceiverConnectionFactory WithInitialPosition(this IReceiverConnectionFactory connectionFactory, EventPosition initalPosition)
        {
            var factory = GetImplementation(connectionFactory);
            factory.InitialPosition = initalPosition;
            return factory;
        }

        public static IReceiverConnectionFactory WithInitialEpoch(this IReceiverConnectionFactory connectionFactory, long initialEpoch)
        {
            var factory = GetImplementation(connectionFactory);
            factory.InitialEpoch = initialEpoch;
            return factory;
        }

        public static Task ProcessAsync(
            this Task<IReceiverConnection> receiverConnection,
            ProcessEvents processEvents,
            ProcessErrors processErrors,
            int? maxBatchSize = default,
            TimeSpan? waitTime = default,
            CancellationToken cancellationToken = default) =>
            receiverConnection.ContinueWith(t => t.Result.RunAsync(
                processEvents: processEvents,
                processErrors: processErrors,
                maxBatchSize: maxBatchSize,
                waitTime: waitTime,
                cancellationToken: cancellationToken));

        private static ReliableEventHubReceiverConnectionFactory GetImplementation(IReceiverConnectionFactory @interface) =>
            @interface is ReliableEventHubReceiverConnectionFactory implementation
            ? implementation
            : throw new ArgumentException(
                $"Expecting {nameof(ReliableEventHubReceiverConnectionFactory)} but got a {@interface.GetType().Name}");
    }
}
