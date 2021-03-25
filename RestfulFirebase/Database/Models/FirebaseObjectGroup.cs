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
    public class FirebaseObjectGroup : DistinctGroup<FirebaseObject>, IRealtimeModel
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeWirePath != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebaseObjectGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebaseObjectGroup), value);
        }

        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebaseObjectGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebaseObjectGroup), value);
        }

        #endregion

        #region Initializers

        public static new FirebaseObjectGroup CreateFromKey(string key)
        {
            return new FirebaseObjectGroup(DistinctGroup<FirebaseObject>.CreateFromKey(key));
        }

        public static new FirebaseObjectGroup CreateFromKeyAndEnumerable(string key, IEnumerable<FirebaseObject> properties)
        {
            return new FirebaseObjectGroup(DistinctGroup<FirebaseObject>.CreateFromKeyAndEnumerable(key, properties));
        }

        public FirebaseObjectGroup(IAttributed attributed)
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
                    try
                    {
                        if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamEvent.Path.Length == 1)
                        {
                            var data = streamEvent.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamEvent.Data);
                            foreach (var item in new List<FirebaseObject>(this.Where(i => !data.ContainsKey(i.Key))))
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
                            var obj = this.FirstOrDefault(i => i.Key == streamEvent.Path[1]);
                            var data = streamEvent.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamEvent.Data);
                            var props = data.Select(i => (i.Key, i.Value.ToString()));
                            if (obj == null)
                            {
                                Add(FirebaseObject.CreateFromKeyAndProperties(streamEvent.Path[1], props));
                            }
                            else
                            {
                                obj.PatchRawProperties(props);
                            }
                        }
                        else if (streamEvent.Path.Length == 3)
                        {
                            var obj = this.FirstOrDefault(i => i.Key == streamEvent.Path[1]);
                            var props = new List<(string Key, string Value)>()
                            {
                                (streamEvent.Path[2], streamEvent.Data)
                            };
                            if (obj == null)
                            {
                                Add(FirebaseObject.CreateFromKeyAndProperties(streamEvent.Path[1], props));
                            }
                            else
                            {
                                obj.PatchRawProperties(props);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                });
        }

        public void PatchRawProperties(IEnumerable<(string Key, string Data)> properties)
        {
            foreach (var property in properties)
            {
                try
                {
                    var obj = this.FirstOrDefault(i => i.Key.Equals(property.Key));

                    var data = property.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(property.Data);
                    var props = data.Select(i => (i.Key, i.Value.ToString()));
                    if (obj == null)
                    {
                        obj = FirebaseObject.CreateFromKeyAndProperties(property.Key, props);
                        Add(obj);
                    }
                    else
                    {
                        obj.PatchRawProperties(props);
                    }
                    obj.RealtimeWirePath = Path.Combine(RealtimeWirePath, property.Key);
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
