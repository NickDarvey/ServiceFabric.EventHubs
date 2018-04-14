using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using NickDarvey.ServiceFabric.EventHubs.Schema;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class States
    {
        private const string CheckpointStateFormat = "event-hubs/{0}/consumer-groups/{1}/checkpoints";
        private const string LeaseStateFormat = "event-hubs/{0}/consumer-groups/{1}/leases";

        public static Task<IReliableDictionary2<string, Checkpoint>> GetCheckpointState(
            IReliableStateManager stateManager,
            string eventHubName,
            string consumerGroupName) =>
            stateManager.GetOrAddAsync<IReliableDictionary2<string, Checkpoint>>(
                name: string.Format(CheckpointStateFormat, eventHubName, consumerGroupName));

        public static Task<IReliableDictionary2<string, Lease>> GetLeaseState(
            IReliableStateManager stateManager,
            string eventHubName,
            string consumerGroupName) =>
            stateManager.GetOrAddAsync<IReliableDictionary2<string, Lease>>(
                name: string.Format(LeaseStateFormat, eventHubName, consumerGroupName));
    }
}
