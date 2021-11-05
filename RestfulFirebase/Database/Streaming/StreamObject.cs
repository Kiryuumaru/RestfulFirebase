using Newtonsoft.Json.Linq;
using RestfulFirebase.Utilities;

namespace RestfulFirebase.Database.Streaming
{
    internal class StreamObject
    {
        public JToken JToken { get; }

        public string AbsoluteUrl { get; }

        public string Path { get; }

        public string Url { get; }

        public StreamObject(JToken jToken, string absoluteUrl, string path)
        {
            JToken = jToken;
            AbsoluteUrl = absoluteUrl;
            Path = path;
            Url = string.IsNullOrEmpty(path) ? absoluteUrl : UrlUtilities.Combine(absoluteUrl, path);
        }
    }
}
