using Newtonsoft.Json;
using RestfulFirebase.Common;
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

        public RealtimeWire RealtimeWire
        {
            get => Holder.GetAttribute<RealtimeWire>(nameof(RealtimeWire), nameof(FirebasePropertyGroup)).Value;
            private set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebasePropertyGroup), value);
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

        #endregion

        #region Methods

        public void Delete()
        {
            foreach (var prop in new List<FirebaseProperty>(this))
            {
                prop.Delete();
                this.Remove(prop);
            }
        }

        public void BuildRealtimeWire(FirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeWire = new RealtimeWire(query,
                () =>
                {
                    foreach (var prop in this)
                    {
                        var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                        prop.BuildRealtimeWire(childQuery, invokeSetFirst);
                        prop.RealtimeWire.StartRealtime();
                    }
                },
                () =>
                {
                    foreach (var prop in this)
                    {
                        prop.RealtimeWire?.StopRealtime();
                    }
                },
                streamObject =>
                {
                    bool hasChanges = false;
                    try
                    {
                        if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamObject.Path.Length == 1)
                        {
                            var data = streamObject.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamObject.Data);
                            var blobs = data.Select(i => (i.Key, i.Value.ToString()));
                            foreach (var prop in this.Where(i => !blobs.Any(j => j.Key == i.Key)))
                            {
                                if (prop.RealtimeWire.ConsumeStream(new StreamObject(null, prop.Key))) hasChanges = true;
                            }
                            foreach (var blob in blobs)
                            {
                                try
                                {
                                    var prop = this.FirstOrDefault(i => i.Key == blob.Key);

                                    if (prop == null)
                                    {
                                        if (invokeSetFirst)
                                        {
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => blob.Key);
                                            childQuery.Put(null, null, ex => OnError(ex));
                                            continue;
                                        }
                                        else
                                        {
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                                            prop = FirebaseProperty.CreateFromKey(blob.Key);
                                            prop.BuildRealtimeWire(childQuery, invokeSetFirst);
                                            prop.RealtimeWire.StartRealtime();
                                            prop.RealtimeWire.ConsumeStream(new StreamObject(blob.Item2, blob.Key));
                                            this.Add(prop);
                                            hasChanges = true;
                                        }
                                    }
                                    else
                                    {
                                        if (prop.RealtimeWire.ConsumeStream(new StreamObject(blob.Item2, blob.Key)))
                                        {
                                            hasChanges = true;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    OnError(ex);
                                }
                            }
                        }
                        else if (streamObject.Path.Length == 2)
                        {
                            try
                            {
                                var prop = this.FirstOrDefault(i => i.Key == streamObject.Path[1]);

                                if (prop == null)
                                {
                                    if (streamObject.Data == null) return false;
                                    var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                                    prop = FirebaseProperty.CreateFromKey(streamObject.Path[1]);
                                    prop.BuildRealtimeWire(childQuery, invokeSetFirst);
                                    prop.RealtimeWire.StartRealtime();
                                    prop.RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1]));
                                    this.Add(prop);
                                    hasChanges = true;
                                }
                                else
                                {
                                    if (prop.RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1])))
                                    {
                                        hasChanges = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnError(ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                    return hasChanges;
                });
        }

        #endregion
    }
}
