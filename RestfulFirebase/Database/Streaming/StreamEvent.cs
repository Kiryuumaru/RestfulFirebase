using System;
using System.Collections.Generic;
using System.Linq;
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

        public new StreamEvent Skip(int count)
        {
            return new StreamEvent(Path.Skip(count).ToArray(), Data, EventType, EventSource);
        }
    }
}
