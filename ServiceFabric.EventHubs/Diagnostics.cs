using System.Diagnostics;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticSource CheckpointDiagnostics =
            new DiagnosticListener("NickDarvey.ServiceFabric.EventHubs.Checkpointing");

        public static readonly DiagnosticSource LeaseDiagnostics =
            new DiagnosticListener("NickDarvey.ServiceFabric.EventHubs.Leasing");

        public static readonly DiagnosticSource ReceiveDiagnostics =
            new DiagnosticListener("NickDarvey.ServiceFabric.EventHubs.Receiving");
    }
}
