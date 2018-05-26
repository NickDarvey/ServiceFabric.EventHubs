using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;
using NickDarvey.SampleApplication.Common;

namespace NickDarvey.SampleApplication.ReliableService.AspNetCore
{
    internal static class Program
    {
        private static void Main()
        {
            var _ = new ArgumentException();
            try
            {

                ServiceRuntime.RegisterServiceAsync("ReliableService.AspNetCoreType",
                    context => new ReliableServiceAspNetCore(context)).GetAwaiter().GetResult();
                DiagnosticListener.AllListeners.Subscribe(new DebugListenerObserver());
                Thread.Sleep(Timeout.Infinite);
            }
            catch(Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }
        }
    }
}
