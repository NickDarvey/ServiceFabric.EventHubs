using System;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class Constants
    {
        public const int DefaultMaxBatchSize = 10;
        public static readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromSeconds(5);
    }
}
