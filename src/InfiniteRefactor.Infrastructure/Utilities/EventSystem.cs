using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace AirMaster.Infrastructure.Utilities
{
    [SupportedOSPlatform("windows")]
    public class EventSystem : IDisposable
    {
        private string logger;

        public EventSystem(string logger)
        {
            this.logger = logger;
        }

        private Dictionary<string, EventLog> cache = new Dictionary<string, EventLog>();

        public void Raise(string eventName, string message, EventLogEntryType type = EventLogEntryType.Information, int eventId = 0, short category = 0, byte[] rawData = null)
        {
            var el = Prepare(eventName);
            el.WriteEntry(message, type, eventId, category, rawData);
        }

        private EventLog Prepare(string eventName)
        {
            if (!EventLog.SourceExists(eventName))
            {
                EventLog.CreateEventSource(new EventSourceCreationData(eventName, logger));
            }

            if (!cache.ContainsKey(eventName))
            {
                lock (this)
                {
                    if (!cache.ContainsKey(eventName))
                    {
                        cache[eventName] = new EventLog(logger, ".", eventName);
                    }
                }
            }

            return cache[eventName];
        }

        public void Listen(string eventName, Action<EventLogEntry> entryWritten)
        {
            var log = Prepare(eventName);
            log.EnableRaisingEvents = true;
            log.EntryWritten += new EntryWrittenEventHandler((sender, e) =>
            {
                if (e.Entry.Source == eventName)
                {
                    entryWritten(e.Entry);
                }
            });
        }

        public void Dispose()
        {
            foreach (var item in cache)
            {
                item.Value.Dispose();
            }
        }
    }

}
