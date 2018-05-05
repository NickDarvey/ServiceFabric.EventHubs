using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.EventHubs;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Transformations
    {
        public static HttpRequestMessage Convert(EventData @event)
        {
            var request = new HttpRequestMessage();
            request.Content = new ByteArrayContent(@event.Body.Array);

            foreach (var property in @event.Properties.Concat(@event.SystemProperties))
            {
                switch (property.Value)
                {
                    case DateTime dateTime:
                        request.Headers.TryAddWithoutValidation(property.Key, dateTime.ToString("o"));
                        break;
                    default:
                        request.Headers.TryAddWithoutValidation(property.Key, property.Value.ToString());
                        break;
                }
            }

            return request;
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
