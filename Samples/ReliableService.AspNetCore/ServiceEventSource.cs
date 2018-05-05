using System.Diagnostics.Tracing;

namespace NickDarvey.SampleApplication.ReliableService.AspNetCore
{
    [EventSource(Name = "NickDarvey-SampleApplication-ReliableService.AspNetCore")]
    internal sealed class ServiceEventSource : EventSource
    {
        public static readonly ServiceEventSource Current = new ServiceEventSource();

        private ServiceEventSource() : base() { }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(MessageEventId, message);
            }
        }

        private const int ErrorEventId = 2;
        [Event(ErrorEventId, Level = EventLevel.Error, Message = "{0}")]
        public void Error(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(ErrorEventId, message);
            }
        }
    }
}
