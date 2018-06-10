using System;
using System.Collections.Generic;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public class TestableSystemPropertiesCollection : Dictionary<string, object>
    {
        public const string EnqueuedTimeUtcName = "x-opt-enqueued-time";
        public const string SequenceNumberName = "x-opt-sequence-number";
        public const string OffsetName = "x-opt-offset";
        public const string PublisherName = "x-opt-publisher";
        public const string PartitionKeyName = "x-opt-partition-key";

        /// <summary>Gets the logical sequence number of the event within the partition stream of the Event Hub.</summary>
        public long SequenceNumber => (long)this[SequenceNumberName];

        /// <summary>Gets or sets the date and time of the sent time in UTC.</summary>
        /// <value>The enqueue time in UTC. This value represents the actual time of enqueuing the message.</value>
        public DateTime EnqueuedTimeUtc => (DateTime)this[EnqueuedTimeUtcName];

        /// <summary>
        /// Gets the offset of the data relative to the Event Hub partition stream. The offset is a marker or identifier for an event within the Event Hubs stream. The identifier is unique within a partition of the Event Hubs stream.
        /// </summary>
        public string Offset => (string)this[OffsetName];

        /// <summary>Gets the partition key of the corresponding partition that stored the <see cref="EventData"/></summary>
        public string PartitionKey => (string)this[PartitionKeyName];

        public TestableSystemPropertiesCollection(Dictionary<string, object> properties)
        {
            if (!properties.ContainsKey(SequenceNumberName))
                throw new ArgumentException($"'{SequenceNumberName}' is required");

            if (!properties.ContainsKey(EnqueuedTimeUtcName))
                throw new ArgumentException($"'{EnqueuedTimeUtcName}' is required");

            if (!properties.ContainsKey(OffsetName))
                throw new ArgumentException($"'{OffsetName}' is required");

            if (!properties.ContainsKey(PartitionKeyName))
                throw new ArgumentException($"'{PartitionKeyName}' is required");

            foreach (var property in properties) Add(property.Key, property.Value);
        }
    }
}
