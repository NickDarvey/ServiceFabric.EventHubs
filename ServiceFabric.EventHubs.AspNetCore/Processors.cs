using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Processors
    {
        public static ProcessEvents CreateEventProcessor(HttpClient client, Action<HttpRequestMessage> requestBuilder) =>
            async events =>
            {
                // Execute in series, so the receiver gets them in correct order.
                foreach(var @event in events)
                {
                    await Process(@event, client, requestBuilder, Transformations.Convert);
                }
            };

        public static ProcessErrors CreateErrorProcessor(HttpClient client, Action<HttpRequestMessage> requestBuilder) =>
            error => Process(error, client, requestBuilder, Transformations.Convert);

        private static async Task Process<T>(T value, HttpClient client, Action<HttpRequestMessage> requestBuilder, Func<T, HttpRequestMessage> converter)
        {
            var req = converter(value);

            requestBuilder(req);

            if (req.RequestUri == null) throw new ArgumentException(
                "You must specify a HTTP request URI (request.RequestUri) using the request builder.");

            if (req.Method == null) throw new ArgumentException(
                "You must specify a HTTP method (request.Method) using the request builder.");

            var resp = await client.SendAsync(req);
            resp.EnsureSuccessStatusCode();
        }
    }
}
