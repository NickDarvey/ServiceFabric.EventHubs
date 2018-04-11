namespace NickDarvey.ServiceFabric.EventHubs.Schema
{
    public struct Lease
    {
        public long Epoch { get; }

        public Lease(long epoch) =>
            Epoch = epoch;

        public override string ToString() =>
            nameof(Lease) + "(" +
            nameof(Epoch) + ": " + Epoch + ")";
    }
}
