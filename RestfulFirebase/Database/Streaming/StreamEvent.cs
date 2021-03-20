using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamEvent : StreamObject
    {
        public EventSource EventSource { get; }

        public EventType EventType { get; }

        public StreamEvent(string key, string data, EventType eventType, EventSource eventSource)
            : base(key, data)
        {
            EventType = eventType;
            EventSource = eventSource;
        }

        public static StreamEvent Empty(EventSource source) => new StreamEvent(string.Empty, default, EventType.InsertOrUpdate, source);
    }
}
