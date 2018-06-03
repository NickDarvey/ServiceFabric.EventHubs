using Microsoft.Azure.EventHubs;
using System;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Partitions
    {
        public static async Task<string> GetPartitionId(long partitionKey, Uri serviceName, FabricClient fabricClient, ITestableEventHubClient eventHubClient)
        {
            var serviceFabricPartitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);
            var eventHubInformation = await eventHubClient.GetRuntimeInformationAsync();

            if (serviceFabricPartitions.Count != eventHubInformation.PartitionCount)
            {
                var msg = $"EventListener partitions ({serviceFabricPartitions.Count}) did not match the Event Hubs partitions ({eventHubInformation.PartitionCount}).";
                throw new ArgumentOutOfRangeException(msg);
            }

            if (partitionKey < 0 || partitionKey > eventHubInformation.PartitionIds.Length)
            {
                var msg = $"EventListener partition key ({partitionKey}) did not fit within the Event Hubs partitions range ({eventHubInformation.PartitionIds.First()} -- {eventHubInformation.PartitionIds.Last()}).";
                throw new IndexOutOfRangeException(msg);
            }

            var partitionId = eventHubInformation.PartitionIds[partitionKey];

            return partitionId;
        }
    }
}
