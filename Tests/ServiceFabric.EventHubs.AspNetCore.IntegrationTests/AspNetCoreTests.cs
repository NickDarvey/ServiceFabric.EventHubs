using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventHubs;
using Microsoft.ServiceFabric.Services.Runtime;
using Moq;
using NickDarvey.ServiceFabric.EventHubs;
using ServiceFabric.Mocks;
using Xunit;

namespace ServiceFabric.EventHubs
{
    public class AspNetCoreTests
    {
        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Category", "Integration")]
        public async Task Test()
        {
            var count = default(int);
            var message = Encoding.UTF8.GetBytes("Message");
            var @event = new EventData(message);
            @event.Properties["Test"] = "Test";
            var props = new TestableEventData.TestableSystemPropertiesCollection(0, DateTime.MinValue, "x", "0");
            var events = new TestableEventData[] { new TestableEventData(@event, props) };
            var received = Task.FromResult<IEnumerable<TestableEventData>>(events);
            var info = Task.FromResult(new EventHubRuntimeInformation()
            {
                PartitionCount = 1,
                PartitionIds = new[] { "0" }
            });
            var cts = new CancellationTokenSource();
            var receiver = new Mock<ITestablePartitionReceiver>();
            var client = new Mock<ITestableEventHubClient>();
            var ctx = MockStatefulServiceContextFactory.Default;
            var state = new MockReliableStateManager();
            var service = new Mock<StatefulService>(ctx, state);
            var builder = WebHost.CreateDefaultBuilder()
                .UseUrls("http://*:5000")
                .Configure(app => app.Run(c => { count++; return c.Response.WriteAsync("Received"); }));

            receiver.Setup(r => r.ReceiveAsync(
                It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .Returns(received);

            client.Setup(c => c.GetRuntimeInformationAsync())
                .Returns(info);
            client.Setup(c => c.CreateEpochReceiver(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventPosition>(), It.IsAny<long>(), It.IsAny<ReceiverOptions>()))
                .Returns(receiver.Object);

            var target = Task.Run(() => new ReliableEventHubReceiverConnectionFactory(
                client: client.Object,
                state: state,
                handlers: checkpointer => (ev, err) => new BatchCheckpointEventHandler(ev, err, checkpointer),
                partitions: pk => Task.FromResult(pk.ToString()))
                .CreateReceiver(0, "Test")
                .ProcessAsync(
                    webHostBuilder: builder,
                    eventRequestBuilder: req =>
                    {
                        req.RequestUri = new Uri("/", UriKind.Relative);
                        req.Method = HttpMethod.Get;
                    },
                    cancellationToken: cts.Token));

            await Task.Delay(60000);
            cts.Cancel();

            // Seems to get ~120,00 on my Surface Book 2
            Assert.True(count > 100_000, "Evaluated " + count);
        }
    }
}
