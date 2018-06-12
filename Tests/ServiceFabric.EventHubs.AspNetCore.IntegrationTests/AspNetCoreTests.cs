using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventHubs;
using Moq;
using NFluent;
using NickDarvey.ServiceFabric.EventHubs;
using ServiceFabric.Mocks;
using Xunit;
using static NickDarvey.ServiceFabric.EventHubs.TestableSystemPropertiesCollection;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public class AspNetCoreTests
    {
        private static readonly string SampleMessageContent = "Message";
        private static readonly byte[] SampleMessage = Encoding.UTF8.GetBytes(SampleMessageContent);
        private static readonly TestableSystemPropertiesCollection SampleProperties = new TestableSystemPropertiesCollection(new Dictionary<string, object> {
            { SequenceNumberName, 1L },
            { EnqueuedTimeUtcName, DateTime.MinValue },
            { OffsetName, "x" },
            { PartitionKeyName, "id" }});
        private static readonly TestableEventData SampleEvent = new TestableEventData(new EventData(SampleMessage), SampleProperties);
        private static readonly IEnumerable<TestableEventData> SampleEvents = new[] { SampleEvent };

        private static ReliableEventHubReceiverConnectionFactory CreateTarget(IEnumerable<Unit> loop, IEnumerable<TestableEventData> events)
        {
            var receiver = new Mock<ITestablePartitionReceiver>();
            receiver.Setup(r => r.ReceiveAsync(
                It.IsAny<int>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(events));

            var client = new Mock<ITestableEventHubClient>();
            client.Setup(c => c.GetRuntimeInformationAsync())
                .Returns(Task.FromResult(new EventHubRuntimeInformation() { PartitionCount = 1, PartitionIds = new[] { "0" } }));
            client.Setup(c => c.CreateEpochReceiver(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventPosition>(), It.IsAny<long>(), It.IsAny<ReceiverOptions>()))
                .Returns(receiver.Object);

            var state = new MockReliableStateManager();

            return new ReliableEventHubReceiverConnectionFactory(
                client: client.Object,
                state: state,
                handlers: checkpointer => (ev, err) => new BatchCheckpointEventHandler(ev, err, checkpointer),
                partitions: pk => Task.FromResult(pk.ToString()),
                initialPosition: EventPosition.FromEnd(),
                initialEpoch: 0,
                loop: loop);
        }


        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Category", "Integration")]
        public async Task Should_be_performant()
        {
            var count = default(int);
            var sw = Stopwatch.StartNew();
            var loop = Enumerable.Repeat(Unit.Default, 100_000);
            var builder = WebHost.CreateDefaultBuilder()
                .UseUrls("http://*:5000")
                .Configure(app => app.Run(c => { count++; return c.Response.WriteAsync("Received"); }));


            await CreateTarget(loop, SampleEvents)
                .CreateReceiver(0, "Test")
                .ProcessAsync(
                    webHostBuilder: builder,
                    eventRequestBuilder: req =>
                    {
                        req.RequestUri = new Uri("/", UriKind.Relative);
                        req.Method = HttpMethod.Get;
                    });

            // Seems to get ~2,000 events/second on my Surface Book 2 with the 'best performance' profile.\
            //  Processor    Intel(R) Core(TM) i7-8650U CPU @ 1.90GHz, 2112 Mhz, 4 Core(s), 8 Logical Processor(s)
            //  Installed Physical Memory (RAM)    16.0 GB
            Check.That(sw.Elapsed.TotalSeconds).IsStrictlyLessThan(60);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Should_send_process_request()
        {
            var result = default(byte[]);
            var builder = WebHost.CreateDefaultBuilder()
                .UseUrls("http://*:5000")
                .Configure(app => app.Run(c =>
                {
                    result = ((MemoryStream)c.Request.Body).ToArray();
                    c.Response.StatusCode = StatusCodes.Status200OK;
                    return c.Response.WriteAsync("Yup");
                }));


            await CreateTarget(new[] { Unit.Default }, SampleEvents)
                .CreateReceiver(0, "Test")
                .ProcessAsync(
                    webHostBuilder: builder,
                    eventRequestBuilder: req =>
                    {
                        req.RequestUri = new Uri("/events", UriKind.Relative);
                        req.Method = HttpMethod.Post;
                    });


            Check.That(result).ContainsExactly(SampleMessage);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Should_send_system_properties_as_process_request_headers()
        {
            var result = default(IDictionary<string, string>);
            var builder = WebHost.CreateDefaultBuilder()
                .UseUrls("http://*:5000")
                .Configure(app => app.Run(c =>
                {
                    result = c.Request.Headers.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
                    c.Response.StatusCode = StatusCodes.Status200OK;
                    return c.Response.WriteAsync("Yup");
                }));


            await CreateTarget(new[] { Unit.Default }, SampleEvents)
                .CreateReceiver(0, "Test")
                .ProcessAsync(
                    webHostBuilder: builder,
                    eventRequestBuilder: req =>
                    {
                        req.RequestUri = new Uri("/events", UriKind.Relative);
                        req.Method = HttpMethod.Post;
                    });


            Check.That(result[SequenceNumberName]).IsEqualTo(SampleProperties.SequenceNumber.ToString());
            Check.That(result[EnqueuedTimeUtcName]).IsEqualTo(SampleProperties.EnqueuedTimeUtc.ToString("o"));
            Check.That(result[OffsetName]).IsEqualTo(SampleProperties.Offset);
            Check.That(result[PartitionKeyName]).IsEqualTo(SampleProperties.PartitionKey);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Should_send_poison_request_when_process_request_is_rejected()
        {
            var result = default(byte[]);
            var builder = WebHost.CreateDefaultBuilder()
                .UseUrls("http://*:5000")
                .Configure(app => app.Run(c =>
                {
                    if (c.Request.Path.Value == "/events")
                    {
                        c.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return c.Response.WriteAsync("Nope");
                    }

                    else if (c.Request.Path.Value == "/poison")
                    {
                        result = ((MemoryStream)c.Request.Body).ToArray();
                        c.Response.StatusCode = StatusCodes.Status200OK;
                        return c.Response.WriteAsync("Yup");
                    }

                    else
                    {
                        throw new NotImplementedException("Path not valid");
                    }
                }));


            await CreateTarget(new[] { Unit.Default }, SampleEvents)
                .CreateReceiver(0, "Test")
                .ProcessAsync(
                    webHostBuilder: builder,
                    eventRequestBuilder: req =>
                    {
                        req.RequestUri = new Uri("/events", UriKind.Relative);
                        req.Method = HttpMethod.Post;
                    },
                    poisonRequestBuilder: (err, req) =>
                    {
                        req.RequestUri = new Uri("/poison", UriKind.Relative);
                        req.Method = HttpMethod.Post;
                    });


            Check.That(result).ContainsExactly(SampleMessage);
        }
    }
}
