using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public class InMemoryServer : IServer
    {
        public InMemoryServer(IFeatureCollection features = null) =>
            Features = features ?? new FeatureCollection();

        public IFeatureCollection Features { get; }
        private IHttpApplication<HostingApplication.Context> Application { get; set; }


        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            Application = (IHttpApplication<HostingApplication.Context>)application;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose() { }

        public HttpClient CreateHttpClient()
        {
            var handler = new ClientHandler(PathString.Empty, Application);
            return new HttpClient(handler);
        }
    }
}
