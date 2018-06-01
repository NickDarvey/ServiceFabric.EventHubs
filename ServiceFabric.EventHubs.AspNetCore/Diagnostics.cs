using System.Diagnostics;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticSource ProcessDiagnostics =
            new DiagnosticListener("NickDarvey.ServiceFabric.EventHubs.Processing");
    }
}
