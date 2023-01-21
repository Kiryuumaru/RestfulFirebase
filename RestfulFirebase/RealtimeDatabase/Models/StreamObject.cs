using RestfulFirebase.Common.Utilities;
using System.Text.Json;

namespace RestfulFirebase.RealtimeDatabase.Models;

internal class StreamObject
{
    public JsonElement JsonElement { get; }

    public string StreamUrl { get; }

    public string Url { get; }

    public string[] Path { get; }

    public StreamObject(JsonElement jsonElement, string streamUrl, string[] path)
    {
        JsonElement = jsonElement;
        StreamUrl = streamUrl;
        Path = path;
        Url = path.Length == 0 ? streamUrl : UrlUtilities.Combine(streamUrl, path);
    }
}
