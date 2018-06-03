using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using NickDarvey.ServiceFabric.EventHubs.Schema;
using static NickDarvey.ServiceFabric.EventHubs.Diagnostics;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal class BatchCheckpointEventHandler : IEventHandler
    {
        private readonly ProcessEvents _processEvents;
        private readonly ProcessErrors _processErrors;
        private readonly SaveCheckpoint _checkpoint;

        public BatchCheckpointEventHandler(
            ProcessEvents processEvents,
            ProcessErrors processErrors,
            SaveCheckpoint checkpoint)
        {
            _processEvents = processEvents;
            _processErrors = processErrors;
            _checkpoint = checkpoint;
        }

        public async Task ProcessEventsAsync(IEnumerable<TestableEventData> events)
        {
            await _processEvents(events).ConfigureAwait(false);

            var last = events.LastOrDefault();

            if (last == default(TestableEventData)) return;

            var checkpoint = new Checkpoint(
                sequenceNumber: last.SystemProperties.SequenceNumber,
                offset: last.SystemProperties.Offset);

            var activity = last.ExtractActivity("Checkpoint");
            if (CheckpointDiagnostics.IsEnabled("Checkpoint"))
                CheckpointDiagnostics.StartActivity(activity, new { Checkpoint = checkpoint });

            await _checkpoint(checkpoint).ConfigureAwait(false);

            if (CheckpointDiagnostics.IsEnabled("Checkpoint"))
                CheckpointDiagnostics.StopActivity(activity, new { Checkpoint = checkpoint });
        }

        public Task ProcessErrorAsync(Exception error) =>
            _processErrors(error);
        
    }
}
