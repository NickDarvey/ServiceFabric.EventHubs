using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Azure.EventHubs;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public class TestableEventData : IDisposable
    {
        private readonly EventData _event;

        /// <summary>
        /// Construct EventData to send to EventHub.
        /// Typical pattern to create a Sending EventData is:
        /// <para>i.  Serialize the sending ApplicationEvent to be sent to EventHub into bytes.</para>
        /// <para>ii. If complex serialization logic is involved (for example: multiple types of data) - add a Hint using the <see cref="EventData.Properties"/> for the Consumer.</para>
        /// </summary>
        /// <example>Sample Code:
        /// <code>
        /// EventData eventData = new EventData(new ArraySegment&lt;byte&gt;(eventBytes, offset, count));
        /// eventData.Properties["eventType"] = "com.microsoft.azure.monitoring.EtlEvent";
        /// await partitionSender.SendAsync(eventData);
        /// </code>
        /// </example>
        /// <param name="arraySegment">The payload bytes, offset and length to be sent to the EventHub.</param>
        public TestableEventData(EventData @event)
        {
            _event = @event;
            SystemProperties = @event.SystemProperties != default
                ? new TestableSystemPropertiesCollection(@event.SystemProperties)
                : throw new ArgumentNullException(nameof(@event.SystemProperties));
        }

        internal TestableEventData(EventData @event, TestableSystemPropertiesCollection systemProperties)
        {
            _event = @event;
            SystemProperties = systemProperties;
        }

        /// <summary>
        /// Get the actual Payload/Data wrapped by EventData.
        /// This is intended to be used after receiving EventData using <see cref="PartitionReceiver"/>.
        /// </summary>
        public ArraySegment<byte> Body => _event.Body;

        /// <summary>
        /// Application property bag
        /// </summary>
        public IDictionary<string, object> Properties => _event.Properties;

        /// <summary>
        /// SystemProperties that are populated by EventHubService.
        /// As these are populated by Service, they are only present on a Received EventData.
        /// </summary>
        public TestableSystemPropertiesCollection SystemProperties { get; }

        public Activity ExtractActivity(string activityName) =>
            _event.ExtractActivity(activityName);

        public void Dispose() =>
            _event.Dispose();
    }
}
