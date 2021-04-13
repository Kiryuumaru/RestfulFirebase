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

        }

        public void StartRealtime(FirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeWire = new RealtimeWire(query,
                () =>
                {
                    foreach (var prop in this)
                    {
                        var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                        prop.StartRealtime(childQuery, invokeSetFirst);
                    }
                },
                streamObject =>
                {
                    try
                    {
                        if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamObject.Path.Length == 1)
                        {
                            var data = streamObject.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamObject.Data);
                            var props = data.Select(i => (i.Key, i.Value.ToString()));
                            foreach (var propHolder in this.Where(i => !props.Any(j => j.Key == i.Property.Key)))
                            {
                                DeleteProperty(propHolder.Property.Key);
                            }
                            foreach (var prop in props)
                            {
                                try
                                {
                                    bool hasChanges = false;

                                    var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(streamObject.Path[0]));

                                    if (propHolder == null)
                                    {
                                        propHolder = new PropertyHolder()
                                        {
                                            Property = PropertyFactory(DistinctProperty.CreateFromKey(prop.Key)),
                                            Group = null,
                                            PropertyName = null
                                        };
                                        PropertyHolders.Add(propHolder);
                                        hasChanges = true;
                                    }

                                    ((FirebaseProperty)propHolder.Property).RealtimeWire.ConsumeStream(new StreamObject(streamObject.Skip(1).Path, prop.Item2));

                                    if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
                                }
                                catch (Exception ex)
                                {
                                    OnError(ex);
                                }
                            }
                        }
                        else if (streamObject.Path.Length == 2)
                        {
                            var props = new List<(string, string)>()
                            {
                                (streamObject.Path[1], streamObject.Data)
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
                    var childQuery = new ChildQuery(RealtimeWire.App, RealtimeWire, () => property.Key);
                    var propHolder = this.FirstOrDefault(i => i.Key.Equals(property.Key));
                    if (propHolder == null)
                    {
                        propHolder = FirebaseProperty.CreateFromKeyAndBlob(property.Key, property.Data);
                        propHolder.RealtimeWire = childQuery;
                        Add(propHolder);
                    }
                    else
                    {
                        propHolder.RealtimeWire = childQuery;
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
