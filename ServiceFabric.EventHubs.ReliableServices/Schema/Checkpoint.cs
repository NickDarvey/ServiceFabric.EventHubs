namespace NickDarvey.ServiceFabric.EventHubs.Schema
{
    public struct Checkpoint
    {
        public long SequenceNumber { get; }
        public string Offset { get; }

        public Checkpoint(long sequenceNumber, string offset) =>
            (SequenceNumber, Offset) = (sequenceNumber, offset);

        public override string ToString() =>
            nameof(Checkpoint) + "(" +
            nameof(SequenceNumber) + ": " + SequenceNumber + ", " +
            nameof(Offset) + ": " + Offset + ")";
    }
}
