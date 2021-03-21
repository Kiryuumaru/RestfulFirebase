using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamEvent : StreamObject
    {
        public EventSource EventSource { get; }

        public EventType EventType { get; }

        public StreamEvent(string[] path, string data, EventType eventType, EventSource eventSource)
            : base(path, data)
        {
            EventType = eventType;
            EventSource = eventSource;
        }
    }
}
