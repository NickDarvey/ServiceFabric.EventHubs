using Microsoft.Azure.EventHubs;
using NickDarvey.ServiceFabric.EventHubs.Schema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public delegate Task ProcessEvents(IEnumerable<TestableEventData> events);
    public delegate Task ProcessErrors(Exception ex);
    internal delegate Task SaveCheckpoint(Checkpoint checkpoint);
    internal delegate IEventHandler CreateHandler(ProcessEvents processEvents, ProcessErrors processErrors);
    internal struct Unit { public static readonly Unit Default = new Unit(); }

}
