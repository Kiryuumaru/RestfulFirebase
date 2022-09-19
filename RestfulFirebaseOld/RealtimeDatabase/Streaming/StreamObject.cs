namespace RestfulFirebase.RealtimeDatabase.Streaming;

using RestfulFirebase.Utilities;
using System.Text.Json;

internal class StreamObject
{
    public JsonElement JsonElement { get; }

    public string AbsoluteUrl { get; }

    public string Path { get; }

    public string Url { get; }

    public StreamObject(JsonElement jsonElement, string absoluteUrl, string path)
    {
        JsonElement = jsonElement;
        AbsoluteUrl = absoluteUrl;
        Path = path;
        Url = string.IsNullOrEmpty(path) ? absoluteUrl : UrlUtilities.Combine(absoluteUrl, path);
    }
}
