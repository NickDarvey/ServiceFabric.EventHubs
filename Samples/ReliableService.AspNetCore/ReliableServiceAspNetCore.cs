using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NickDarvey.ServiceFabric.EventHubs;

namespace NickDarvey.SampleApplication.ReliableService.AspNetCore
{

    internal sealed class ReliableServiceAspNetCore : StatefulService
    {
        private readonly string ConnectionString;
        private readonly string ConsumerGroupName;

        public ReliableServiceAspNetCore(StatefulServiceContext context)
            : base(context)
        {
            var config = Context.CodePackageActivationContext
                .GetConfigurationPackageObject("Config").Settings
                .Sections["EventHubs"];

            ConnectionString = config
                .Parameters["ConnectionString"]
                .Value;

            ConsumerGroupName = config
                .Parameters["ConsumerGroupName"]
                .Value;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() =>
            new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, (url, listener) =>
                CreateWebHostBuilder()
                .ConfigureServices(services => services
                    .AddSingleton(serviceContext)
                    .AddSingleton(StateManager))
                .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                .UseUrls(url)
                .Build()))
            };

        protected override Task RunAsync(CancellationToken cancellationToken) =>
            // Create the Event Hub client, as you usually would.
            EventHubClient.CreateFromConnectionString(ConnectionString)

            // Pass in the state manager, we'll use this to do our checkpointing.
            .UseServiceFabricState(this)

            // Pick the style of checkpointing to use.
            .WithBatchCheckpointing()

            // Create a connection to an Event Hub partition
            .CreateReceiver(
                partitionKey: ((Int64RangePartitionInformation)Partition.PartitionInfo).LowKey,
                consumerGroupName: ConsumerGroupName,
                cancel: cancellationToken)

            // Start processing events
            .ProcessAsync(
                webHostBuilder: CreateWebHostBuilder(),
                eventRequestBuilder: req =>
                {
                    req.RequestUri = new Uri("/test/events", UriKind.Relative);
                    req.Method = HttpMethod.Post;
                },
                poisonRequestBuilder: (err, req) =>
                {
                    req.RequestUri = new Uri("/test/poison", UriKind.Relative);
                    req.Method = HttpMethod.Post;
                    req.Headers.TryAddWithoutValidation("X-Poison-StatusCode", err.StatusCode.ToString());
                },
                processErrors: error => { ServiceEventSource.Current.Error(error.ToString()); return Task.CompletedTask; },
                cancellationToken: cancellationToken);

        private static IWebHostBuilder CreateWebHostBuilder() =>
            new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>();
    }
}
