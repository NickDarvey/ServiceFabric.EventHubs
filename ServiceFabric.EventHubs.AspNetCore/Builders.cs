using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public static class Builders
    {
        public static async Task ProcessAsync(
            this Task<IReceiverConnection> receiver,
            IWebHostBuilder webHostBuilder,
            Action<HttpRequestMessage> eventRequestBuilder,
            Action<HttpResponseMessage, HttpRequestMessage> poisonRequestBuilder = default,
            ProcessErrors processErrors = default,
            int? maxBatchSize = default,
            TimeSpan? waitTime = default,
            CancellationToken cancellationToken = default)
        {
            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            {
                var events = Processors.CreateSerialEventProcessor(client, eventRequestBuilder, poisonRequestBuilder);
                await receiver.ProcessAsync(events, processErrors, maxBatchSize, waitTime, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
