using Microsoft.Azure.EventHubs;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public static class Builders
    {
        public static IReceiverConnectionFactory WithServiceFabricIntegration(this EventHubClient client, StatefulService service) =>
            client.UseServiceFabricState(service).WithBatchCheckpointing();

        public struct ServiceFabricEventHubsBuilder
        {
            internal EventHubClient Client { get; }
            internal StatefulService Service { get; }

            internal ServiceFabricEventHubsBuilder(EventHubClient client, StatefulService service)
            {
                Client = client;
                Service = service;
            }
        }

        public static ServiceFabricEventHubsBuilder UseServiceFabricState(this EventHubClient client, StatefulService service) =>
            new ServiceFabricEventHubsBuilder(client, service);

        public static IReceiverConnectionFactory WithBatchCheckpointing(this ServiceFabricEventHubsBuilder builder) =>
            new ReliableEventHubReceiverConnectionFactory(builder.Client, builder.Service.StateManager, builder.Service.Context.ServiceName, checkpointer => (events, errors) => new BatchCheckpointEventHandler(events, errors, checkpointer));

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
                processErrors:  processErrors ?? (_ => Task.CompletedTask),
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
