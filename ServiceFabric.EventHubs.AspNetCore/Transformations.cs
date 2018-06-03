using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.EventHubs;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Transformations
    {
        public static HttpRequestMessage Convert(TestableEventData @event)
        {
            var request = new HttpRequestMessage();
            request.Content = new ByteArrayContent(@event.Body.Array);
            Append(request.Content.Headers, @event.Properties);
            Append(request.Headers, @event.SystemProperties);
            return request;
        }

        private static void Append(HttpHeaders headers, IDictionary<string, object> properties)
        {
            if (properties == default) return;

            foreach (var property in properties)
            {
                switch (property.Value)
                {
                    case DateTime dateTime:
                        headers.TryAddWithoutValidation(property.Key, dateTime.ToString("o"));
                        break;
                    default:
                        headers.TryAddWithoutValidation(property.Key, property.Value.ToString());
                        break;
                }
            }
        }

        public static HttpRequestMessage Convert(Exception exception)
        {
            var request = new HttpRequestMessage();
            request.Content = new StringContent(exception.ToString());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            return request;
        }

    }
}
