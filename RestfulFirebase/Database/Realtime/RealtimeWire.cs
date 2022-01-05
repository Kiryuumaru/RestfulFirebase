using Newtonsoft.Json.Linq;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Realtime
{
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

        internal EventHandler<StreamObject> Next;

        private IDisposable subscription;
        private bool hasFirstStream;

        #endregion

        #region Initializers

        internal RealtimeWire(RestfulFirebaseApp app, IFirebaseQuery query, ILocalDatabase localDatabase)
            : base(app, query, localDatabase)
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

        private void OnNext(object sender, StreamObject streamObject)
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                Next?.Invoke(sender, streamObject);

                string[] path = UrlUtilities.Separate(streamObject.Path);

                if (streamObject.JToken.Type == JTokenType.Null)
                {
                    MakeSync(default(string), path);
                }
                else if (streamObject.JToken is JValue jValue)
                {
                    MakeSync(jValue.ToString(), path);
                }
                else if (streamObject.JToken is JObject || streamObject.JToken is JArray)
                {
                    IDictionary<string[], object> pairs = streamObject.JToken.GetFlatHierarchy();
                    Dictionary<string[], string> values = new Dictionary<string[], string>(pairs.Count, PathEqualityComparer.Instance);
                    if (path.Length == 0)
                    {
                        foreach (KeyValuePair<string[], object> pair in pairs)
                        {
                            values.Add(pair.Key, pair.Value.ToString());
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string[], object> pair in pairs)
                        {
                            string[] subPath = new string[pair.Key.Length + path.Length];
                            Array.Copy(path, 0, subPath, 0, path.Length);
                            Array.Copy(pair.Key, 0, subPath, path.Length - 1, pair.Key.Length);
                            values.Add(subPath, pair.Value.ToString());
                        }
                    }
                    MakeSync(values, path);
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

        #endregion

        #region RealtimeInstance Members

        /// <inheritdoc/>
        public override RealtimeInstance Clone()
        {
            if (IsDisposed)
            {
                return default;
            }

            var clone = new RealtimeWire(App, Query, LocalDatabase);
            clone.SyncOperation.SetContext(this);

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
}
