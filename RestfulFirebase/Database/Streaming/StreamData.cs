using RestfulFirebase.Utilities;
using System.Collections.Generic;

namespace RestfulFirebase.Database.Streaming
{
    internal class StreamData
    {
        protected StreamData()
        {

        }
    }

    internal class SingleStreamData : StreamData
    {
        public string Blob { get; }

        public SingleStreamData(string blob)
        {
            Blob = blob;
        }
    }

    internal class MultiStreamData : StreamData
    {
        public Dictionary<string, StreamData> Blobs { get; }

        public MultiStreamData(Dictionary<string, StreamData> blobs)
        {
            Blobs = blobs;
        }

        public IEnumerable<(string path, string blob)> GetDescendants()
        {
            var descendants = new List<(string path, string blob)>();

            void recursive(Dictionary<string, StreamData> blobs, string path)
            {
                foreach (var pair in blobs)
                {
                    if (pair.Value is null)
                    {
                        if (string.IsNullOrEmpty(path)) descendants.Add((UrlUtilities.Combine(pair.Key), null));
                        else descendants.Add((UrlUtilities.Combine(path, pair.Key), null));
                    }
                    else if (pair.Value is SingleStreamData single)
                    {
                        if (string.IsNullOrEmpty(path)) descendants.Add((UrlUtilities.Combine(pair.Key), single.Blob));
                        else descendants.Add((UrlUtilities.Combine(path, pair.Key), single.Blob));
                    }
                    else if (pair.Value is MultiStreamData multi)
                    {
                        if (string.IsNullOrEmpty(path)) recursive(multi.Blobs, UrlUtilities.Combine(pair.Key));
                        else recursive(multi.Blobs, UrlUtilities.Combine(path, pair.Key));
                    }
                }
            }

            recursive(Blobs, "");

            return descendants;
        }
    }
}
