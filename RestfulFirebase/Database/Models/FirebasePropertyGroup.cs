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

        public bool HasRealtimeWire => RealtimeWirePath != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebasePropertyGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebasePropertyGroup), value);
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

        }

        public void SetRealtime(IFirebaseQuery query, bool invokeSetFirst, out Action<StreamEvent> onNext)
        {
            RealtimeWirePath = query.GetAbsolutePath();
            onNext = new Action<StreamEvent>(streamEvent =>
            {
                try
                {
                    if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                    else if (streamEvent.Path.Length == 1)
                    {
                        var data = streamEvent.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamEvent.Data);
                        var props = data.Select(i => (i.Key, i.Value.ToString()));
                        ReplaceRawProperties(props);
                    }
                    else if (streamEvent.Path.Length == 2)
                    {
                        var props = new List<(string, string)>()
                            {
                                (streamEvent.Path[1], streamEvent.Data)
                            };
                        UpdateRawProperties(props);
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            });
        }

        protected void UpdateRawProperties(IEnumerable<(string Key, string Data)> properties)
        {
            foreach (var property in properties)
            {
                try
                {
                    var propHolder = this.FirstOrDefault(i => i.Key.Equals(property.Key));
                    if (propHolder == null)
                    {
                        propHolder = FirebaseProperty.CreateFromKeyAndBlob(property.Key, property.Data);
                        propHolder.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, property.Key);
                        Add(propHolder);
                    }
                    else
                    {
                        propHolder.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, property.Key);
                        if (propHolder.Blob != property.Data)
                        {
                            propHolder.UpdateBlob(property.Data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        protected void ReplaceRawProperties(IEnumerable<(string Key, string Data)> properties)
        {
            foreach (var prop in new List<FirebaseProperty>(this.Where(i => !properties.Any(j => j.Key == i.Key))))
            {
                Remove(prop);
            }
            UpdateRawProperties(properties);
        }

        #endregion
    }
}
