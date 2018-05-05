using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.TestHost;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public static class Builders
    {
        public static Task ProcessAsync(
            this Task<IReceiverConnection> receiver,
            InMemoryServer server,
            Action<HttpRequestMessage> processEventRequestBuilder,
            Action<HttpRequestMessage> processErrorRequestBuilder,
            int? maxBatchSize = default,
            TimeSpan? waitTime = default,
            CancellationToken cancellationToken = default)
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://localhost:19080") })
            {
                var events = Processors.CreateEventProcessor(client, processEventRequestBuilder);
                var errors = Processors.CreateErrorProcessor(client, processEventRequestBuilder);

                return receiver.ProcessAsync(events, errors, maxBatchSize, waitTime, cancellationToken);
            }
        }
    }
}
