using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public static class ServiceFabricEventHubClient
    {
        public static async Task<EventHubClient> CreateFromSubscription(
            Uri serviceUri,
            long partitionKey,
            string subscriptionName)
        {
            // TODO: Check if subscription already exists?

            var subscribable = ServiceProxy.Create<ISubscribable>(serviceUri, new ServicePartitionKey(partitionKey));
            var request = new SubscriptionRequest(subscriptionName);
            var response = await subscribable.Subscribe(request);

            return EventHubClient.CreateFromConnectionString(response.ConnectionString);
        }
    }
}
