using Microsoft.Azure.EventHubs;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public static class Builders
    {
        public static IReceiverConnectionFactory WithServiceFabricIntegration(this Task<EventHubClient> client, StatefulService service) =>
            client.UseServiceFabricState(service).WithBatchCheckpointing();

        public struct ServiceFabricEventHubsBuilder
        {
            internal Task<ITestableEventHubClient> Client { get; }
            internal StatefulService Service { get; }

            internal ServiceFabricEventHubsBuilder(Task<ITestableEventHubClient> client, StatefulService service)
            {
                Client = client;
                Service = service;
            }
        }

        public static ServiceFabricEventHubsBuilder UseServiceFabricState(this EventHubClient client, StatefulService service) =>
            UseServiceFabricState(Task.FromResult(client), service);

        public static ServiceFabricEventHubsBuilder UseServiceFabricState(this Task<EventHubClient> client, StatefulService service) =>
            new ServiceFabricEventHubsBuilder(client.ContinueWith<ITestableEventHubClient>(t => new TestableEventHubClient(t.Result)), service);

        internal static ServiceFabricEventHubsBuilder UseServiceFabricState(this Task<ITestableEventHubClient> client, StatefulService service) =>
            new ServiceFabricEventHubsBuilder(client, service);

        public static IReceiverConnectionFactory WithBatchCheckpointing(this ServiceFabricEventHubsBuilder builder) =>
            new ReliableEventHubReceiverConnectionFactory(
                client: builder.Client,
                state: builder.Service.StateManager,
                handlers: checkpointer => (events, errors) => new BatchCheckpointEventHandler(events, errors, checkpointer),
                partitions: async pk =>
                {
                    var client = await builder.Client.ConfigureAwait(false);
                    using (var c = new FabricClient())
                    {
                        return await Partitions.GetPartitionId(pk, builder.Service.Context.ServiceName, c, client).ConfigureAwait(false);
                    }
                });

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

        public static async Task ProcessAsync(
            this Task<IReceiverConnection> receiverConnection,
            ProcessEvents processEvents,
            ProcessErrors processErrors = default,
            int? maxBatchSize = default,
            TimeSpan? waitTime = default,
            CancellationToken cancellationToken = default)
        {
            var connection = await receiverConnection.ConfigureAwait(false);
            await connection.RunAsync(
                processEvents: processEvents,
                processErrors: processErrors ?? (_ => Task.CompletedTask),
                maxBatchSize: maxBatchSize,
                waitTime: waitTime,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }


        private static ReliableEventHubReceiverConnectionFactory GetImplementation(IReceiverConnectionFactory @interface) =>
            @interface is ReliableEventHubReceiverConnectionFactory implementation
            ? implementation
            : throw new ArgumentException(
                $"Expecting {nameof(ReliableEventHubReceiverConnectionFactory)} but got a {@interface.GetType().Name}");
    }
}
