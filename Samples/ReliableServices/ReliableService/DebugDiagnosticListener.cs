using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NickDarvey.SampleApplication.ReliableService
{
    internal class DebugListenerObserver : IObserver<DiagnosticListener>
    {
        public void OnCompleted() =>
            Debug.WriteLine("Diagnostics completed");

        public void OnError(Exception error) =>
            Debug.WriteLine(error.Message);

        public void OnNext(DiagnosticListener value) =>
            value.Subscribe(new DebugEventObserver(value.Name));
    }

    internal class DebugEventObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly string _name;

        public DebugEventObserver(string name)
        {
            _name = name;
            Debug.WriteLine(name + ": Started");
        }

        public void OnCompleted() =>
            Debug.WriteLine(_name + ": Completed");

        public void OnError(Exception error) =>
            Debug.WriteLine(_name + ": " + error.Message);

        public void OnNext(KeyValuePair<string, object> value) =>
            Debug.WriteLine($"{_name}: {value.Key} | {value.Value.ToString()}");
    }
}
