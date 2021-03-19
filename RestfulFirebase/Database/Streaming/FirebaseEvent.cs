namespace RestfulFirebase.Database.Streaming
{
    public class FirebaseEvent<T> : StreamObject<T>
    {
        public FirebaseEventSource EventSource { get; }

        public FirebaseEventType EventType { get; }

        public FirebaseEvent(string key, T obj, FirebaseEventType eventType, FirebaseEventSource eventSource)
            : base(key, obj)
        {
            EventType = eventType;
            EventSource = eventSource;
        }

        public static FirebaseEvent<T> Empty(FirebaseEventSource source) => new FirebaseEvent<T>(string.Empty, default, FirebaseEventType.InsertOrUpdate, source);
    }
}
