
using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;

namespace SerilogMask.Core.Logging
{
    public class SerilogInMemorySink : ILogEventSink, IDisposable
    {
        public static readonly SerilogInMemorySink Instance = new();
        private SerilogInMemorySink()
        {
        }
     
        private readonly ConcurrentBag<LogEvent> _logEvents = new();

        public void Emit(LogEvent logEvent)
        {
            _logEvents.Add(logEvent);
        }

        public IReadOnlyCollection<LogEvent> LogEvents => _logEvents;

        public void Clear()
        {
            _logEvents.Clear();
        }

        public void Dispose()
        {
           _logEvents.Clear();
        }
    }
}
   