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
        public static Task ProcessAsync(
            this Task<IReceiverConnection> receiver,
            IWebHostBuilder webHostBuilder,
            Action<HttpRequestMessage> eventRequestBuilder,
            Action<HttpRequestMessage> errorRequestBuilder,
            int? maxBatchSize = default,
            TimeSpan? waitTime = default,
            CancellationToken cancellationToken = default)
        {
            using (var server = new TestServer(webHostBuilder))
            using (var client = server.CreateClient())
            //using (var client = new HttpClient() { BaseAddress = new Uri("http://localhost:19080") })
            {
                var events = Processors.CreateEventProcessor(client, eventRequestBuilder);
                var errors = Processors.CreateErrorProcessor(client, eventRequestBuilder);

                return receiver.ProcessAsync(events, errors, maxBatchSize, waitTime, cancellationToken);
            }
        }
    }
}
