using Newtonsoft.Json;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database.Models
{
    public class FirebasePropertyGroup : DistinctGroup<FirebaseProperty>, IRealtimeModel
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeWirePath != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebasePropertyGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebasePropertyGroup), value);
        }

        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebasePropertyGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebasePropertyGroup), value);
        }

        #endregion

        #region Initializers

        public static new FirebasePropertyGroup CreateFromKey(string key)
        {
            return new FirebasePropertyGroup(DistinctGroup<FirebaseProperty>.CreateFromKey(key));
        }

        public static new FirebasePropertyGroup CreateFromKeyAndEnumerable(string key, IEnumerable<FirebaseProperty> properties)
        {
            return new FirebasePropertyGroup(DistinctGroup<FirebaseProperty>.CreateFromKeyAndEnumerable(key, properties));
        }

        public FirebasePropertyGroup(IAttributed attributed)
            : base(attributed)
        {

        }

        public void Dispose()
        {
            RealtimeSubscription?.Dispose();
        }

        #endregion

        #region Methods

        public void SetStreamer(IFirebaseQuery query)
        {
            RealtimeWirePath = query.GetAbsolutePath();
            RealtimeSubscription = Observable
                .Create<StreamEvent>(observer => new NodeStreamer(observer, query, (s, e) => OnError(e)).Run())
                .Subscribe(streamEvent =>
                {
                    if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                    else if (streamEvent.Path.Length == 1)
                    {
                        var data = streamEvent.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamEvent.Data);
                        foreach (var item in new List<FirebaseProperty>(this.Where(i => !data.ContainsKey(i.Key))))
                        {
                            Remove(item);
                        }
                        if (data.Count != 0)
                        {
                            var props = data.Select(i => (i.Key, i.Value.ToString()));
                            PatchRawProperties(props);
                        }
                    }
                    else if (streamEvent.Path.Length == 2)
                    {
                        var prop = this.FirstOrDefault(i => i.Key == streamEvent.Path[1]);
                        if (prop == null)
                        {
                            Add(FirebaseProperty.CreateFromKeyAndBlob(streamEvent.Path[1], null));
                        }
                        else
                        {
                            prop.Update(streamEvent.Data);
                        }
                    }
                });
        }

        protected void PatchRawProperties(IEnumerable<(string Key, string Data)> properties)
        {
            foreach (var property in properties)
            {
                try
                {
                    var propHolder = this.FirstOrDefault(i => i.Key.Equals(property.Key));

                    if (propHolder == null)
                    {
                        propHolder = FirebaseProperty.CreateFromKeyAndBlob(property.Key, property.Data);
                        Add(propHolder);
                    }
                    else
                    {
                        if (propHolder.Data != property.Data)
                        {
                            propHolder.Update(property.Data);
                        }
                    }
                    propHolder.RealtimeWirePath = Path.Combine(RealtimeWirePath, property.Key);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        #endregion
    }
}
