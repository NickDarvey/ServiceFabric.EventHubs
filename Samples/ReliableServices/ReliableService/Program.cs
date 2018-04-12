using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;

namespace NickDarvey.SampleApplication.ReliableService
{
    internal static class Program
    {
        private static void Main()
        {
            ServiceRuntime.RegisterServiceAsync("ReliableServiceType",
                context => new ReliableService(context)).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
