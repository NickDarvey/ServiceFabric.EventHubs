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

1. Install the NuGet package to your stateful service.

    `Install-Package NickDarvey.ServiceFabric.EventHubs.ReliableServices`

1. Create a receiver and start processing events during `RunAsync`

    ```csharp
    internal sealed class ReliableService : StatefulService
    {
        public ReliableService(StatefulServiceContext context)
            : base(context)
        { }

        protected override Task RunAsync(CancellationToken cancellationToken) =>
            EventHubClient.CreateFromConnectionString("Endpoint=sb://...")
            .UseServiceFabricState(StateManager)
            .WithBatchCheckpointing()
            .CreateReceiver(PartitionKey, "$Default", cancellationToken)
            .ProcessAsync(
                events => DoWork(events),
                error => LogError(error),
                cancellationToken: cancellationToken);
    }
    ```

Check out [the samples](./Samples) for more details.


# Usage
There are at least two usage models you should consider for integrating Event Hubs with Service Fabric.

One is processing events in the same service in which you receive events (co-location),
the other is processing events in a service and getting an Event Hubs-specific service deployed alongside (sidecar).

I've tried these (and a number of other) ways of integration and the co-location pattern proved to be optimal, because of:

* Performance
  
  In-process method beats network calls.
  Part of the reason you probably chose Service Fabric was that state was kept with compute.
  If you care about that, you probably care about reducing the network hops too.

* Consistency
  
  Back up is per service, per partition. If you ever have to recover state, you don't have a snapshot across services.
  The checkpoint state of the Event Hubs service and the state your event processing service won't be consistent.
  If you co-locate them, they will be.

The downside is you are coupling to Event Hubs partitioning scheme.
You are coupled because you are receiving events per Event Hub partition which uses a [perfect hash function](./) to partition incoming events.
If you're updating state in your service based on events (and not simply flinging them onto another Event Hub or similar) and want to read it then you'll need to use the same function to figure out which partition your data is on.
(I don't think it couples you to Event Hubs itself because this package is trivial to rip out.)


# Roadmap
* Perfect Hash function implementation

  so you can figure out which partition your data is on.

* Stateful ASP.NET integration

  so you can process events as if it were a HTTP request (but all happening in-process)

* Reliable Actors integration
  
  so you can hand off events to actors for processing


