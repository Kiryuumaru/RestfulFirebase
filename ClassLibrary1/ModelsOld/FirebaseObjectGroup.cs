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
    public class FirebaseObjectGroup : DistinctGroup<FirebaseObject>, IRealtimeModel
    {
        #region Properties

        public RealtimeWire RealtimeWire
        {
            get => Holder.GetAttribute<RealtimeWire>(nameof(RealtimeWire), nameof(FirebaseObjectGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebaseObjectGroup), value);
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

        #endregion

        #region Methods

        protected virtual FirebaseObject ObjectFactory<T>(T property, string tag = null)
            where T : DistinctObject
        {
            return new FirebaseObject(property);
        }

        protected override void InsertItem(int index, FirebaseObject item)
        {
            var prop = ObjectFactory(item);
            if (RealtimeWire != null)
            {
                var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                prop.BuildRealtimeWire(childQuery);
                prop.RealtimeWire.StartRealtime();
            }
            base.InsertItem(index, prop);
        }

        protected override void SetItem(int index, FirebaseObject item)
        {
            var prop = ObjectFactory(item);
            if (RealtimeWire != null)
            {
                var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                prop.BuildRealtimeWire(childQuery);
                prop.RealtimeWire.StartRealtime();
            }
            base.SetItem(index, prop);
        }

        public void Delete()
        {
            foreach (var prop in new List<FirebaseObject>(this))
            {
                prop.Delete();
                this.Remove(prop);
            }
        }

        public void BuildRealtimeWire(FirebaseQuery query)
        {
            bool invokeSetFirst = false;
            RealtimeWire = new RealtimeWire(query,
                () =>
                {
                    foreach (var prop in this)
                    {
                        var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                        prop.BuildRealtimeWire(childQuery);
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
                            foreach (var prop in new List<FirebaseObject>(this.Where(i => !blobs.Any(j => j.Key == i.Key))))
                            {
                                this.Remove(prop);
                                if (prop.RealtimeWire.ConsumeStream(new StreamObject(null, prop.Key))) hasChanges = true;
                            }
                            foreach (var blob in blobs)
                            {
                                try
                                {
                                    var obj = this.FirstOrDefault(i => i.Key == blob.Key);

                                    if (obj == null)
                                    {
                                        if (invokeSetFirst && !RealtimeWire.HasFirstStream)
                                        {
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => blob.Key);
                                            childQuery.Put(null, null, ex => OnError(ex));
                                            continue;
                                        }
                                        else
                                        {
                                            obj = ObjectFactory(FirebaseObject.CreateFromKey(blob.Key));
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => obj.Key);
                                            obj.BuildRealtimeWire(childQuery);
                                            obj.RealtimeWire.ConsumeStream(new StreamObject(blob.Item2, blob.Key));
                                            obj.RealtimeWire.StartRealtime();
                                            this.Add(obj);
                                            hasChanges = true;
                                        }
                                    }
                                    else
                                    {
                                        if (invokeSetFirst && obj.GetRawPersistableProperties().All(i => i.Blob == null) && !RealtimeWire.HasFirstStream)
                                        {
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => blob.Key);
                                            childQuery.Put(null, null, ex => OnError(ex));
                                            continue;
                                        }
                                        else
                                        {
                                            if (obj.RealtimeWire.ConsumeStream(new StreamObject(blob.Item2, blob.Key)))
                                            {
                                                hasChanges = true;
                                            }
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
                                var obj = this.FirstOrDefault(i => i.Key == streamObject.Path[1]);

                                if (obj == null)
                                {
                                    if (streamObject.Data == null) return false;
                                    obj = ObjectFactory(FirebaseObject.CreateFromKey(streamObject.Path[1]));
                                    var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => obj.Key);
                                    obj.BuildRealtimeWire(childQuery);
                                    obj.RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1]));
                                    obj.RealtimeWire.StartRealtime();
                                    this.Add(obj);
                                    hasChanges = true;
                                }
                                else
                                {
                                    if (streamObject.Data == null) this.Remove(obj);
                                    if (obj.RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1])))
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
                        else if (streamObject.Path.Length == 3)
                        {
                            try
                            {
                                var obj = this.FirstOrDefault(i => i.Key == streamObject.Path[1]);

                                if (obj == null)
                                {
                                    if (streamObject.Data == null) return false;
                                    obj = ObjectFactory(FirebaseObject.CreateFromKey(streamObject.Path[1]));
                                    var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => obj.Key);
                                    obj.BuildRealtimeWire(childQuery);
                                    obj.RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1]));
                                    obj.RealtimeWire.StartRealtime();
                                    this.Add(obj);
                                    hasChanges = true;
                                }
                                else
                                {
                                    if (streamObject.Data == null) this.Remove(obj);
                                    if (obj.RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1])))
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
