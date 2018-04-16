using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;

namespace NickDarvey.SampleApplication.ReliableService
{
    internal static class Program
    {
        private static void Main()
        {
            var _ = new ArgumentException();

            ServiceRuntime.RegisterServiceAsync("ReliableServiceType",
                context => new ReliableService(context)).GetAwaiter().GetResult();
            DiagnosticListener.AllListeners.Subscribe(new DebugListenerObserver());
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
