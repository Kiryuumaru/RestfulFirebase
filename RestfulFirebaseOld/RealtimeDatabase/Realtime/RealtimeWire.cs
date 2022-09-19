namespace RestfulFirebase.RealtimeDatabase.Realtime;

using RestfulFirebase.RealtimeDatabase.Query;
using RestfulFirebase.RealtimeDatabase.Streaming;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// The base query subscribing fluid implementations for firebase realtime database.
/// </summary>
public class RealtimeWire : RealtimeInstance
{
    #region Properties

    /// <inheritdoc/>
    public override bool HasFirstStream => hasFirstStream;

    /// <inheritdoc/>
    public override bool Started => subscription != null;

    internal EventHandler<StreamObject>? Next;

    private IDisposable? subscription;
    private bool hasFirstStream;

    #endregion

    #region Initializers

    internal RealtimeWire(RestfulFirebaseApp app, IFirebaseQuery query)
        : base(app, query)
    {

    }

    #endregion

    #region Methods

    /// <summary>
    /// Start to subscribe the wire to the node.
    /// </summary>
    public void Start()
    {
        if (IsDisposed)
        {
            return;
        }

        if (subscription == null)
        {
            hasFirstStream = false;
            string uri = Query.GetAbsoluteUrl();
            subscription = new NodeStreamer(App, Query, OnNext, (s, e) => OnError(e.Url, e.Exception)).Run();
        }
    }

    /// <summary>
    /// Unsubscribe the wire to the node.
    /// </summary>
    public void Stop()
    {
        if (IsDisposed)
        {
            return;
        }

        if (subscription != null)
        {
            hasFirstStream = false;
            subscription.Dispose();
            subscription = null;
        }
    }

    private void OnNext(object? sender, StreamObject streamObject)
    {
        if (IsDisposed)
        {
            return;
        }

        try
        {
            Next?.Invoke(sender, streamObject);

            string[] path = UrlUtilities.Separate(streamObject.Path);

            switch (streamObject.JsonElement.ValueKind)
            {
                case JsonValueKind.Null:
                    MakeSync(default(string), path);
                    break;
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    MakeSync(GetFlatHierarchy(streamObject.JsonElement), path);
                    break;
                default:
                    MakeSync(streamObject.JsonElement.GetString(), path);
                    break;
            }
        }
        catch (Exception ex)
        {
            OnError(streamObject.Url, ex);
        }

        if (!HasFirstStream)
        {
            hasFirstStream = true;
        }
    }

    private static IDictionary<string[], string?> GetFlatHierarchy(JsonElement jsonElement, bool removeArrayNulls = true)
    {
        Dictionary<string[], string?> descendants = new(PathEqualityComparer.Instance);

        void recursive(JsonElement recToken, string[] path)
        {
            switch (recToken.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var subToken in recToken.EnumerateObject())
                    {
                        string[] subPath = new string[path.Length + 1];
                        Array.Copy(path, 0, subPath, 0, path.Length);
                        subPath[^1] = subToken.Name;
                        recursive(subToken.Value, subPath);
                    }
                    break;
                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var subToken in recToken.EnumerateArray())
                    {
                        if (!removeArrayNulls || subToken.ValueKind != JsonValueKind.Null)
                        {
                            string[] subPath = new string[path.Length + 1];
                            Array.Copy(path, 0, subPath, 0, path.Length);
                            subPath[^1] = index.ToString();
                            recursive(subToken, subPath);
                        }
                        index++;
                    }
                    break;
                case JsonValueKind.Null:
                    descendants.Add(path, null);
                    break;
                default:
                    descendants.Add(path, recToken.GetString());
                    break;
            }
        }

        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var subToken in jsonElement.EnumerateObject())
                {
                    string[] subPath = new string[] { subToken.Name };
                    recursive(subToken.Value, subPath);
                }
                break;
            case JsonValueKind.Array:
                int index = 0;
                foreach (var subToken in jsonElement.EnumerateArray())
                {
                    if (!removeArrayNulls || subToken.ValueKind != JsonValueKind.Null)
                    {
                        string[] subPath = new string[] { index.ToString() };
                        recursive(subToken, subPath);
                    }
                    index++;
                }
                break;
            case JsonValueKind.Null:
                descendants.Add(Array.Empty<string>(), null);
                break;
            default:
                descendants.Add(Array.Empty<string>(), jsonElement.GetString());
                break;
        }

        return descendants;
    }

    #endregion

    #region RealtimeInstance Members

    /// <inheritdoc/>
    public override RealtimeInstance Clone()
    {
        VerifyNotDisposed();

        var clone = new RealtimeWire(App, Query);

        Next += clone.OnNext;
        Disposing += delegate
        {
            Next -= clone.OnNext;
            clone.Dispose();
        };

        return clone;
    }

    #endregion

    #region Disposable Members

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stop();
        }
        base.Dispose(disposing);
    }

    #endregion
}
