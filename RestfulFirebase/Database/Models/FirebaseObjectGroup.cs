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

        public void SetRealtime(IFirebaseQuery query, bool invokeSetFirst)
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
                            var objs = data.Select(i => (i.Key, i.Value.ToString(), new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                            {
                                if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                {
                                    firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, i.Key, firebaseProperty.Key);
                                }
                            })));
                            ReplaceRawObjects(objs);
                        }
                        else if (streamEvent.Path.Length == 2)
                        {
                            var objs = new List<(string, string, Action<(bool HasChanges, PropertyHolder PropertyHolder)>)>()
                            {
                                (streamEvent.Path[1], streamEvent.Data, new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                                {
                                    if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                    {
                                        firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, streamEvent.Path[1], firebaseProperty.Key);
                                    }
                                }))
                            };
                            UpdateRawObjects(objs);
                        }
                        else if (streamEvent.Path.Length == 3)
                        {
                            var props = new List<(string Key, string Value)>()
                            {
                                (streamEvent.Path[2], streamEvent.Data)
                            };
                            var obj = this.FirstOrDefault(i => i.Key == streamEvent.Path[1]);
                            if (obj == null)
                            {
                                obj = FirebaseObject.CreateFromKey(streamEvent.Path[1]);
                                obj.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, obj.Key, streamEvent.Path[1]);
                                obj.ReplaceRawProperties(props, new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                                {
                                    if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                    {
                                        firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, streamEvent.Path[1], firebaseProperty.Key);
                                    }
                                }));
                                Add(obj);
                            }
                            else
                            {
                                obj.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, streamEvent.Path[1]);
                                obj.UpdateRawProperties(props, new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                                {
                                    if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                    {
                                        firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, streamEvent.Path[1], firebaseProperty.Key);
                                    }
                                }));
                            }
                            foreach (FirebaseProperty prop in obj.GetRawPersistableProperties())
                            {
                                prop.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, prop.Key);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                });
        }

        public void UpdateRawObjects(IEnumerable<(string Key, string Data, Action<(bool HasChanges, PropertyHolder Property)> perItemFollowup)> objs)
        {
            foreach (var datas in objs)
            {
                try
                {
                    var data = datas.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(datas.Data);
                    var props = data.Select(i => (i.Key, i.Value.ToString()));
                    var obj = this.FirstOrDefault(i => i.Key.Equals(datas.Key));
                    if (obj == null)
                    {
                        obj = FirebaseObject.CreateFromKey(datas.Key);
                        obj.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, obj.Key);
                        obj.ReplaceRawProperties(props, datas.perItemFollowup);
                        Add(obj);
                    }
                    else
                    {
                        obj.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, obj.Key);
                        obj.ReplaceRawProperties(props, datas.perItemFollowup);
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        public void ReplaceRawObjects(IEnumerable<(string Key, string Data, Action<(bool HasChanges, PropertyHolder PropertyHolder)> perItemFollowup)> objs)
        {
            foreach (var obj in new List<FirebaseObject>(this.Where(i => !objs.Any(j => j.Key == i.Key))))
            {
                Remove(obj);
            }
            UpdateRawObjects(objs);
        }

        #endregion
    }
}
