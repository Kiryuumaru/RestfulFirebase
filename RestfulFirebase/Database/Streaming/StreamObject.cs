using RestfulFirebase.Utilities;

namespace RestfulFirebase.Database.Streaming
{
    internal class StreamObject
    {
        public StreamData Data { get; }

        public string AbsoluteUrl { get; }

        public string Path { get; }

        public string Url { get; }

        public StreamObject(StreamData data, string absoluteUrl, string path)
        {
            Data = data;
            AbsoluteUrl = absoluteUrl;
            Path = path;
            Url = path == "/" ? absoluteUrl : UrlUtilities.Combine(absoluteUrl, path.Substring(1));
        }
    }
}
