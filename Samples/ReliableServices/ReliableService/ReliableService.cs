using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.ServiceFabric.Services.Runtime;
using NickDarvey.ServiceFabric.EventHubs;

namespace NickDarvey.SampleApplication.ReliableService
{
    internal sealed class ReliableService : StatefulService
    {
        public ReliableService(StatefulServiceContext context)
            : base(context)
        { }

        protected override Task RunAsync(CancellationToken cancellationToken) =>

            // Create the Event Hub client, as you usually would.
            EventHubClient.CreateFromConnectionString("")

            // Pass in the state manager, we'll use this to do our checkpointing.
            .UseServiceFabricState(StateManager)

            // Pick the style of checkpointing to use.
            .WithBatchCheckpointing()

            // Create a connection to an Event Hub partition
            .CreateReceiver(
                partitionKey: ((Int64RangePartitionInformation)Partition.PartitionInfo).LowKey,
                consumerGroupName: "$Default",
                cancel: cancellationToken)

            // Start processing events
            .ProcessAsync(
                events => { ServiceEventSource.Current.Message("Received " + events.Count() + " events"); return Task.CompletedTask; },
                error => { ServiceEventSource.Current.Error(error.ToString()); return Task.CompletedTask; },
                cancellationToken: cancellationToken);

    }
}
