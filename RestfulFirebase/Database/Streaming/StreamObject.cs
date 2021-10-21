namespace RestfulFirebase.Database.Streaming
{
    internal class StreamObject
    {
        public StreamData Data { get; }

        public string Uri { get; }

        public StreamObject(StreamData data, string uri)
        {
            Data = data;
            Uri = uri;
        }
    }
}
