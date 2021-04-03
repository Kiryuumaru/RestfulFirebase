using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamEvent : StreamObject
    {
        public EventType EventType { get; }

        public EventSource EventSource { get; }

        internal StreamEvent(string[] path, string data, EventType eventType, EventSource eventSource)
            : base(path, data)
        {
            EventType = eventType;
            EventSource = eventSource;
        }

        public new StreamEvent Clone()
        {
            return new StreamEvent(Path, Data, EventType, EventSource);
        }
    }
}
