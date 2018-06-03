A Service Fabric Reliable Services and Event Hubs integration for perfect partitioning.

```
Event Hub partitions            [0]    [1]    [2]    [4]
                                 ▼      ▼      ▼      ▼
Stateful service partitions     [0]    [1]    [2]    [4]
|  This library
|  Your code
```

Event Hubs is a great entry point for a bajillion messages coming into your Service Fabric application.
This package provides the glue to directly connect a service to Event Hubs, process events and checkpoint as it goes.


# Getting started

## Reliable Services

1. Install the NuGet package to your stateful service.

    `Install-Package NickDarvey.ServiceFabric.EventHubs`

1. Create a receiver and start processing events during `RunAsync`.

    ```csharp
    internal sealed class ReliableService : StatefulService
    {
        public ReliableService(StatefulServiceContext context)
            : base(context)
        { }

        protected override Task RunAsync(CancellationToken cancellationToken) =>

            // Create the Event Hub client, as you usually would.
            EventHubClient.CreateFromConnectionString("Endpoint=sb://...")

            // Pass in the state manager, we'll use this to do our checkpointing.
            .UseServiceFabricState(this)

            // Pick the style of checkpointing to use.
            .WithBatchCheckpointing()

            // Create a connection to an Event Hub partition
            .CreateReceiver(
                partitionKey: ((Int64RangePartitionInformation)Partition.PartitionInfo).LowKey,
                consumerGroupName: ConsumerGroupName,
                cancel: cancellationToken)

            // Start processing events
            .ProcessAsync(
                events => DoWork(events),
                error => LogError(error),
                cancellationToken: cancellationToken);
    }
    ```

1. Start the service with the same partitioning as your Event Hub.
   e.g. `New-ServiceFabricService -ApplicationName fabric:/SampleApplication -ServiceTypeName SampleServiceType -ServiceName fabric:/SampleApplication/SampleService -Stateful -PartitionSchemeUniformInt64 -PartitionCount 32 -LowKey 0 -HighKey 31`

## Reliable Services (ASP.NET Core)

1. Install the NuGet package to your stateful service.

    `Install-Package NickDarvey.ServiceFabric.EventHubs.AspNetCore`

1. Create a receiver and start processing events during `RunAsync`.

    ```csharp
    internal sealed class ReliableServiceAspNetCore : StatefulService
    {
        public ReliableServiceAspNetCore(StatefulServiceContext context)
            : base(context)
        { }

        protected override Task RunAsync(CancellationToken cancellationToken) =>
            // Create the Event Hub client, as you usually would.
            EventHubClient.CreateFromConnectionString("Endpoint=sb://...")

            // Pass in the state manager, we'll use this to do our checkpointing.
            .UseServiceFabricState(this)

            // Pick the style of checkpointing to use.
            .WithBatchCheckpointing()

            // Create a connection to an Event Hub partition
            .CreateReceiver(
                partitionKey: ((Int64RangePartitionInformation)Partition.PartitionInfo).LowKey,
                consumerGroupName: ConsumerGroupName,
                cancel: cancellationToken)

            // Start processing events
            .ProcessAsync(
                webHostBuilder: new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>(),
                eventRequestBuilder: req =>
                {
                    req.RequestUri = new Uri("/test/events", UriKind.Relative);
                    req.Method = HttpMethod.Post;
                },
                poisonRequestBuilder: (err, req) =>
                {
                    req.RequestUri = new Uri("/test/poison", UriKind.Relative);
                    req.Method = HttpMethod.Post;
                },
                processErrors: error => { ServiceEventSource.Current.Error(error.ToString()); return Task.CompletedTask; },
    }
    ```
    
1. Start the service with the same partitioning as your Event Hub.
   e.g. `New-ServiceFabricService -ApplicationName fabric:/SampleApplication -ServiceTypeName SampleServiceType -ServiceName fabric:/SampleApplication/SampleService -Stateful -PartitionSchemeUniformInt64 -PartitionCount 32 -LowKey 0 -HighKey 31`


Check out [the samples](./Samples) for more details.

# Usage
## Partition resolution
You can use the [Event Hub partition key resolver](./ServiceFabric.EventHubs.Partitioning) in other services to resolve on which partition your data resides.

If you're doing partition resolution in Azure API Management, add this as a Named Value called 'Partitioning_PerfectHash':
```csharp
@{ string partitionKey = (string)context.Variables["partitionKey"]; int partitionCount = 32; const short DefaultLogicalPartitionCount = short.MaxValue; uint seed1 = 0; uint seed2 = 0; uint hash1; uint hash2; string upper = partitionKey.ToUpper(); byte[] data = ASCIIEncoding.ASCII.GetBytes(upper); uint a, b, c; a = b = c = (uint)(0xdeadbeef + data.Length + seed1); c += seed2; int index = 0, size = data.Length; while (size > 12) { a += BitConverter.ToUInt32(data, index); b += BitConverter.ToUInt32(data, index + 4); c += BitConverter.ToUInt32(data, index + 8); a -= c; a ^= (c << 4) | (c >> 28); c += b; b -= a; b ^= (a << 6) | (a >> 26); a += c; c -= b; c ^= (b << 8) | (b >> 24); b += a; a -= c; a ^= (c << 16) | (c >> 16); c += b; b -= a; b ^= (a << 19) | (a >> 13); a += c; c -= b; c ^= (b << 4) | (b >> 28); b += a; index += 12; size -= 12; } var shift = true; switch (size) { case 12: a += BitConverter.ToUInt32(data, index); b += BitConverter.ToUInt32(data, index + 4); c += BitConverter.ToUInt32(data, index + 8); break; case 11: c += ((uint)data[index + 10]) << 16; goto case 10; case 10: c += ((uint)data[index + 9]) << 8; goto case 9; case 9: c += (uint)data[index + 8]; goto case 8; case 8: b += BitConverter.ToUInt32(data, index + 4); a += BitConverter.ToUInt32(data, index); break; case 7: b += ((uint)data[index + 6]) << 16; goto case 6; case 6: b += ((uint)data[index + 5]) << 8; goto case 5; case 5: b += ((uint)data[index + 4]); goto case 4; case 4: a += BitConverter.ToUInt32(data, index); break; case 3: a += ((uint)data[index + 2]) << 16; goto case 2; case 2: a += ((uint)data[index + 1]) << 8; goto case 1; case 1: a += (uint)data[index]; break; case 0: hash1 = c; hash2 = b; shift = false; break; } if (shift) { c ^= b; c -= (b << 14) | (b >> 18); a ^= c; a -= (c << 11) | (c >> 21); b ^= a; b -= (a << 25) | (a >> 7); c ^= b; c -= (b << 16) | (b >> 16); a ^= c; a -= (c << 4) | (c >> 28); b ^= a; b -= (a << 14) | (a >> 18); c ^= b; c -= (b << 24) | (b >> 8); } hash1 = c; hash2 = b; long hashedValue = hash1 ^ hash2; short shortHashedValue = (short)hashedValue; short logicalPartition = Math.Abs((short)(shortHashedValue % DefaultLogicalPartitionCount)); int shortRangeWidth = (int)Math.Floor((decimal)DefaultLogicalPartitionCount / (decimal)(partitionCount)); int remainingLogicalPartitions = DefaultLogicalPartitionCount - (partitionCount * shortRangeWidth); int largeRangeWidth = shortRangeWidth + 1; int largeRangesLogicalPartitions = largeRangeWidth * remainingLogicalPartitions; long partitionIndex = logicalPartition < largeRangesLogicalPartitions ? logicalPartition / largeRangeWidth : remainingLogicalPartitions + ((logicalPartition - largeRangesLogicalPartitions) / shortRangeWidth); return partitionIndex; }
```

and then update your API policy with:
```xml
<policies>
    <inbound>
        <base />
        <set-variable name="partitionKey" value="@((string)context.Request.MatchedParameters["userId"])" />
        <set-variable name="resolvedPartitionKey" value="{{Partitioning_PerfectHash}}" />
        <set-backend-service backend-id="servicefabric" sf-partition-key="@((long)context.Variables["resolvedPartitionKey"])" sf-resolve-condition="{{ServiceFabric_ResolutionCondition}}" sf-service-instance-name="fabric:/SampleApplication/SampleService" />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <redirect-content-urls />
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>
```

# Forces
There are at least two usage models you should consider for integrating Event Hubs with Service Fabric depending on the forces acting on your project.

One is processing events in the same service in which you receive events (co-location, for which you can use this library),
the other is processing events in a service and getting an Event Hubs-specific service deployed alongside (sidecar).

I've tried these (and a number of other) ways of integration and the co-location pattern proved to be optimal, because of:

* Performance
  
  In-process method calls beats network calls.
  Part of the reason you probably chose Service Fabric was that state was kept with compute.
  If you care about that, you probably care about reducing the network hops too.

* Consistency
  
  Back up is per service, per partition. If you ever have to recover state, you don't have a snapshot across services.
  The checkpoint state of the Event Hubs service and the state your event processing service won't be consistent.
  If you co-locate them, they will be.

The downside is you are coupling to Event Hubs partitioning scheme.
You are coupled because you are receiving events per Event Hub partition which uses a [perfect hash function](./ServiceFabric.EventHubs.Partitioning) to partition incoming events.
If you're updating state in your service based on events (and not simply flinging them onto another Event Hub or similar) and want to read it then you'll need to use the same function to figure out which partition your data is on.
(I don't think it couples you to Event Hubs itself because this package is trivial to rip out.)


# Roadmap
* ~~Perfect Hash function implementation~~

  ~~so you can figure out which partition your data is on.~~

* ~~Stateful ASP.NET integration~~

  ~~so you can process events as if it were a HTTP request (but all happening in-process)~~

* Reliable Actors integration
  
  so you can hand off events to actors for processing