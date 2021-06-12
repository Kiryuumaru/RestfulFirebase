using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class StreamData
    {
        internal StreamData()
        {

        }
    }

    public class SingleStreamData : StreamData
    {
        public string Blob { get; }

        internal SingleStreamData(string blob)
        {
            Blob = blob;
        }
    }

    public class MultiStreamData : StreamData
    {
        public Dictionary<string, StreamData> Blobs { get; }

        internal MultiStreamData(Dictionary<string, StreamData> blobs)
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
                        if (string.IsNullOrEmpty(path)) descendants.Add((Utils.UrlCombine(pair.Key), null));
                        else descendants.Add((Utils.UrlCombine(path, pair.Key), null));
                    }
                    else if (pair.Value is SingleStreamData single)
                    {
                        if (string.IsNullOrEmpty(path)) descendants.Add((Utils.UrlCombine(pair.Key), single.Blob));
                        else descendants.Add((Utils.UrlCombine(path, pair.Key), single.Blob));
                    }
                    else if (pair.Value is MultiStreamData multi)
                    {
                        if (string.IsNullOrEmpty(path)) recursive(multi.Blobs, Utils.UrlCombine(pair.Key));
                        else recursive(multi.Blobs, Utils.UrlCombine(path, pair.Key));
                    }
                }
            }

            recursive(Blobs, "");

            return descendants;
        }
    }
}
