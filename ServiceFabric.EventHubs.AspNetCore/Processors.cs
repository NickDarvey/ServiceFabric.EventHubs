using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static NickDarvey.ServiceFabric.EventHubs.Diagnostics;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Processors
    {
        public static ProcessEvents CreateSerialEventProcessor(
            HttpClient client,
            Action<HttpRequestMessage> processRequestBuilder,
            Action<HttpResponseMessage, HttpRequestMessage> poisonRequestBuilder) =>
            async events =>
            {
                // Execute in series, so the receiver gets them in correct order.
                foreach (var @event in events)
                {
                    using (var processResponse = await Process(
                        value: @event,
                        client: client,
                        requestBuilder: processRequestBuilder,
                        converter: Transformations.Convert).ConfigureAwait(false))
                    {
                        if (processResponse.IsSuccessStatusCode) return;

                        // If we have been given no way of handling an error, explode
                        if (poisonRequestBuilder == default) processResponse.EnsureSuccessStatusCode();

                        // Otherwise give them an opportunity to inspect the error
                        if (ProcessDiagnostics.IsEnabled("Error"))
                        {
                            // Read here because the content could be disposed
                            // before the observer has a chance to inspect it.
                            var content = await processResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                            ProcessDiagnostics.Write("Error", new { processResponse.StatusCode, processResponse.ReasonPhrase, Content = content });
                        }

                        using (var poisonResponse = await Process(
                            value: @event,
                            client: client, 
                            requestBuilder: req => poisonRequestBuilder(processResponse, req),
                            converter: Transformations.Convert).ConfigureAwait(false))
                        {
                            poisonResponse.EnsureSuccessStatusCode();
                        }
                    }
                }
            };

        private static async Task<HttpResponseMessage> Process<T>(T value, HttpClient client, Action<HttpRequestMessage> requestBuilder, Func<T, HttpRequestMessage> converter)
        {
            using (var req = converter(value))
            {
                try { requestBuilder(req); }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"The request builder ({requestBuilder}) failed to build a request. {ex.Message}", ex);
                }

                if (req.RequestUri == null) throw new ArgumentException(
                    "You must specify a HTTP request URI (request.RequestUri) using the request builder.");

                if (req.Method == null) throw new ArgumentException(
                    "You must specify a HTTP method (request.Method) using the request builder.");

                return await client.SendAsync(req).ConfigureAwait(false);
            }
        }
    }
}
