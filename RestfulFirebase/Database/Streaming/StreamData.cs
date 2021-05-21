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

    public class SingleStreamData2 : StreamData
    {
        public string Blob { get; }

        internal SingleStreamData2(string blob)
        {
            Blob = blob;
        }
    }

    public class MultiStreamData2 : StreamData
    {
        public Dictionary<string, StreamData> Blobs { get; }

        internal MultiStreamData2(Dictionary<string, StreamData> blobs)
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
                        if (string.IsNullOrEmpty(path)) descendants.Add((Utils.CombineUrl(pair.Key), null));
                        else descendants.Add((Utils.CombineUrl(path, pair.Key), null));
                    }
                    else if (pair.Value is SingleStreamData2 single)
                    {
                        if (string.IsNullOrEmpty(path)) descendants.Add((Utils.CombineUrl(pair.Key), single.Blob));
                        else descendants.Add((Utils.CombineUrl(path, pair.Key), single.Blob));
                    }
                    else if (pair.Value is MultiStreamData2 multi)
                    {
                        if (string.IsNullOrEmpty(path)) recursive(multi.Blobs, Utils.CombineUrl(pair.Key));
                        else recursive(multi.Blobs, Utils.CombineUrl(path, pair.Key));
                    }
                }
            }

            recursive(Blobs, "");

            return descendants;
        }
    }
}
