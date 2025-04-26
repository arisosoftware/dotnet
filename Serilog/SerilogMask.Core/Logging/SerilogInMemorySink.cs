
using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;

namespace SerilogMask.Core.Logging
{
public class SerilogInMemorySink : ILogEventSink
    {
        private static SerilogInMemorySink? _instance; // Add the missing static field
        private readonly List<LogEvent> _events;
        private readonly object _lockObject;

        public static SerilogInMemorySink? Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SerilogInMemorySink();
                }
                return _instance;
            }
        }

        public SerilogInMemorySink()
        {
            _events = new List<LogEvent>();
            _lockObject = new object();
            _instance = this; // Update the static field here
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                return;
            }

            lock (_lockObject)
            {
                _events.Add(logEvent);
            }
        }

        public List<LogEvent> GetAllEvents()
        {
            lock (_lockObject)
            {
                return new List<LogEvent>(_events);
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _events.Clear();
            }
        }

        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _events.Count;
                }
            }
        }
    }
}
   