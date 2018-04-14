using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal interface IEventHandler
    {
        Task ProcessEventsAsync(IEnumerable<EventData> events);
        Task ProcessErrorAsync(Exception error);
    }
}
