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

        public bool HasRealtimeWire => RealtimeWire != null;

        public string RealtimeWirePath => RealtimeWire?.GetAbsolutePath();

        public FirebaseQuery RealtimeWire
        {
            get => Holder.GetAttribute<FirebaseQuery>(nameof(RealtimeWire), nameof(FirebaseObjectGroup)).Value;
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

        public void Delete()
        {

        }

        public void StartRealtime(FirebaseQuery query, bool invokeSetFirst, out Action<StreamObject> onNext)
        {
            RealtimeWire = query;
            onNext = new Action<StreamObject>(streamObject =>
            {
                try
                {
                    if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                    else if (streamObject.Path.Length == 1)
                    {
                        var data = streamObject.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamObject.Data);
                        var objs = data.Select(i => (i.Key, i.Value.ToString(), new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                        {
                            if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                            {
                                var childQuery = new ChildQuery(RealtimeWire.App, RealtimeWire, () => i.Key);
                                childQuery = childQuery.Child(firebaseProperty.Key);
                                firebaseProperty.RealtimeWire = childQuery;
                            }
                        })));
                        ReplaceRawObjects(objs);
                    }
                    else if (streamObject.Path.Length == 2)
                    {
                        var objs = new List<(string, string, Action<(bool HasChanges, PropertyHolder PropertyHolder)>)>()
                        {
                            (streamObject.Path[1], streamObject.Data, new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                            {
                                if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                {
                                    var childQuery = new ChildQuery(RealtimeWire.App, RealtimeWire, () => streamObject.Path[1]);
                                    childQuery = childQuery.Child(firebaseProperty.Key);
                                    firebaseProperty.RealtimeWire = childQuery;
                                }
                            }))
                        };
                        UpdateRawObjects(objs);
                    }
                    else if (streamObject.Path.Length == 3)
                    {
                        var props = new List<(string Key, string Value)>()
                            {
                                (streamObject.Path[2], streamObject.Data)
                            };
                        var obj = this.FirstOrDefault(i => i.Key == streamObject.Path[1]);
                        if (obj == null)
                        {
                            var childQuery = new ChildQuery(RealtimeWire.App, RealtimeWire, () => obj.Key);
                            childQuery = childQuery.Child(streamObject.Path[1]);
                            obj = FirebaseObject.CreateFromKey(streamObject.Path[1]);
                            obj.RealtimeWire = childQuery;
                            obj.ReplaceRawProperties(props, new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                            {
                                if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                {
                                    //firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, streamObject.Path[1], firebaseProperty.Key);
                                }
                            }));
                            Add(obj);
                        }
                        else
                        {
                            //obj.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, streamObject.Path[1]);
                            obj.UpdateRawProperties(props, new Action<(bool HasChanges, PropertyHolder PropertyHolder)>(perItemFollowup =>
                            {
                                if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                {
                                    //firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, streamObject.Path[1], firebaseProperty.Key);
                                }
                            }));
                        }
                        foreach (FirebaseProperty prop in obj.GetRawPersistableProperties())
                        {
                            //prop.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, prop.Key);
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
                        //obj.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, obj.Key);
                        obj.ReplaceRawProperties(props, datas.perItemFollowup);
                        Add(obj);
                    }
                    else
                    {
                        //obj.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, obj.Key);
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
