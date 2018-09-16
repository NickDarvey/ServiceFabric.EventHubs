using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using NickDarvey.ServiceFabric.EventHubs;

namespace NickDarvey.SampleApplication.ReliableService.Subscriber
{
    internal sealed class ReliableServiceSubscriber : StatefulService
    {
        private readonly string ConnectionString;
        private readonly string ConsumerGroupName;
        private readonly Uri SubscribableServiceUri;
        private readonly long PartitionKey;

        public ReliableServiceSubscriber(StatefulServiceContext context)
            : base(context)
        {
            var config = Context.CodePackageActivationContext
                .GetConfigurationPackageObject("Config").Settings
                .Sections["EventHubs"];

            ConnectionString = config
                .Parameters["ConnectionString"]
                .Value;

            ConsumerGroupName = config
                .Parameters["ConsumerGroupName"]
                .Value;

            SubscribableServiceUri = new Uri(config
                .Parameters["SubscribableServiceUri"]
                .Value);

            PartitionKey = ((Int64RangePartitionInformation)Partition.PartitionInfo).LowKey;
        }

        protected override Task RunAsync(CancellationToken cancellationToken) =>

            // Create the Event Hub client from a subscription
            ServiceFabricEventHubClient.CreateFromSubscription(
                serviceUri: SubscribableServiceUri,
                partitionKey: PartitionKey,
                subscriptionName: Context.ServiceName.ToString())

            // Pass in the state manager, we'll use this to do our checkpointing.
            .UseServiceFabricState(this)

            // Pick the style of checkpointing to use.
            .WithBatchCheckpointing()

            // Create a connection to an Event Hub partition
            .CreateReceiver(
                partitionKey: PartitionKey,
                consumerGroupName: ConsumerGroupName,
                cancel: cancellationToken)

            // Start processing events
            .ProcessAsync(
                events => { ServiceEventSource.Current.Message("Received " + events.Count() + " events"); return Task.CompletedTask; },
                error => { ServiceEventSource.Current.Error(error.ToString()); return Task.CompletedTask; },
                cancellationToken: cancellationToken);

    }
}
