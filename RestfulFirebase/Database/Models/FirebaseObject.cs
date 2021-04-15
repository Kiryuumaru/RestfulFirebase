using Newtonsoft.Json;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Converters;
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
    public class FirebaseObject : DistinctObject, IRealtimeModel
    {
        #region Properties

        public RealtimeWire RealtimeWire
        {
            get => Holder.GetAttribute<RealtimeWire>(nameof(RealtimeWire), nameof(FirebaseObject)).Value;
            private set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebaseObject), value);
        }

        public string TypeIdentifier
        {
            get => GetPersistableProperty<string>("_t");
            protected set => SetPersistableProperty(value, "_t");
        }

        public SmallDateTime Modified
        {
            get
            {
                GetPersistableProperty<string>("_m");
                var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key == "_m");
                var prop = (FirebaseProperty)propHolder.Property;
                return prop.Modified;
            }
            set => SetPersistableProperty<string>("", "_m");
        }

        #endregion

        #region Initializers

        public static new FirebaseObject Create()
        {
            return new FirebaseObject(DistinctObject.Create());
        }

        public static new FirebaseObject CreateFromKey(string key)
        {
            return new FirebaseObject(DistinctObject.CreateFromKey(key));
        }

        public static new FirebaseObject CreateFromKeyAndProperties(string key, IEnumerable<(string Key, string Data)> properties)
        {
            return new FirebaseObject(DistinctObject.CreateFromKeyAndProperties(key, properties));
        }

        public FirebaseObject(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods

        protected override DistinctProperty PropertyFactory<T>(T property, string tag = null)
        {
            return new FirebaseProperty(property);
        }

        protected void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = "",
            Func<T, T, bool> validateValue = null)
        {
            SetProperty(value, key, nameof(FirebaseObject), propertyName, validateValue,
                customValueSetter: args =>
                {
                    bool hasChanges = false;
                    var prop = (FirebaseProperty)args.property;
                    if (RealtimeWire != null)
                    {
                        var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                        prop.BuildRealtimeWire(childQuery);
                        hasChanges = prop.ModifyData(DataTypeConverter.GetConverter<T>().Encode(args.value));
                        prop.RealtimeWire.StartRealtime();
                    }
                    else
                    {
                        hasChanges = prop.ModifyData(DataTypeConverter.GetConverter<T>().Encode(args.value));
                    }
                    return hasChanges;
                });
        }

        protected T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = "")
        {
            return GetProperty(key, nameof(FirebaseObject), defaultValue, propertyName,
                customValueSetter: args =>
                {
                    bool hasChanges = false;
                    var prop = (FirebaseProperty)args.property;
                    if (RealtimeWire != null)
                    {
                        var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                        prop.BuildRealtimeWire(childQuery);
                        hasChanges = prop.ModifyData(DataTypeConverter.GetConverter<T>().Encode(args.value));
                        prop.RealtimeWire.StartRealtime();
                    }
                    else
                    {
                        hasChanges = prop.ModifyData(DataTypeConverter.GetConverter<T>().Encode(args.value));
                    }
                    return hasChanges;
                });
        }

        public void Delete()
        {
            foreach (var propHolder in PropertyHolders)
            {
                DeleteProperty(propHolder.Property.Key);
            }
        }

        public void BuildRealtimeWire(FirebaseQuery query)
        {
            bool invokeSetFirst = false;
            RealtimeWire = new RealtimeWire(query,
                () =>
                {
                    foreach (var prop in GetRawPersistableProperties())
                    {
                        var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                        prop.BuildRealtimeWire(childQuery);
                        prop.RealtimeWire.StartRealtime();
                    }
                },
                () =>
                {
                    RealtimeWire = null;
                    foreach (var prop in GetRawPersistableProperties())
                    {
                        var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
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
                            var blobs = data.Select(i => (i.Key, i.Value?.ToString()));
                            foreach (var propHolder in new List<PropertyHolder>(PropertyHolders.Where(i => !blobs.Any(j => j.Key == i.Property.Key))))
                            {
                                if (((FirebaseProperty)propHolder.Property).RealtimeWire.ConsumeStream(new StreamObject(null, propHolder.Property.Key)))
                                {
                                    OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
                                    hasChanges = true;
                                }
                            }
                            foreach (var blob in blobs)
                            {
                                try
                                {
                                    bool hasSubChanges = false;

                                    var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(blob.Key));

                                    if (propHolder == null)
                                    {
                                        if (invokeSetFirst && !RealtimeWire.HasFirstStream)
                                        {
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => blob.Key);
                                            childQuery.Put(null, null, ex => OnError(ex));
                                            continue;
                                        }
                                        else
                                        {
                                            var prop = (FirebaseProperty)PropertyFactory(FirebaseProperty.CreateFromKey(blob.Key, SmallDateTime.MinValue));
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                                            prop.BuildRealtimeWire(childQuery);
                                            prop.RealtimeWire.ConsumeStream(new StreamObject(blob.Item2, blob.Key));
                                            prop.RealtimeWire.StartRealtime();
                                            propHolder = new PropertyHolder()
                                            {
                                                Property = prop,
                                                Group = null,
                                                PropertyName = null
                                            };
                                            PropertyHolders.Add(propHolder);
                                            hasSubChanges = true;
                                        }
                                    }
                                    else
                                    {
                                        if (invokeSetFirst && propHolder.Property.Blob == null && !RealtimeWire.HasFirstStream)
                                        {
                                            var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => blob.Key);
                                            childQuery.Put(null, null, ex => OnError(ex));
                                            continue;
                                        }
                                        else
                                        {
                                            if (((FirebaseProperty)propHolder.Property).RealtimeWire.ConsumeStream(new StreamObject(blob.Item2, blob.Key)))
                                            {
                                                hasSubChanges = true;
                                            }
                                        }
                                    }

                                    if (hasSubChanges)
                                    {
                                        OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
                                        hasChanges = true;
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
                                bool hasSubChanges = false;

                                var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(streamObject.Path[1]));

                                if (propHolder == null)
                                {
                                    if (streamObject.Data == null) return false;
                                    var prop = (FirebaseProperty)PropertyFactory(FirebaseProperty.CreateFromKey(streamObject.Path[1], SmallDateTime.MinValue));
                                    var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                                    prop.BuildRealtimeWire(childQuery);
                                    prop.RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1]));
                                    prop.RealtimeWire.StartRealtime();
                                    propHolder = new PropertyHolder()
                                    {
                                        Property = prop,
                                        Group = null,
                                        PropertyName = null
                                    };
                                    PropertyHolders.Add(propHolder);
                                    hasSubChanges = true;
                                }
                                else
                                {
                                    if (((FirebaseProperty)propHolder.Property).RealtimeWire.ConsumeStream(new StreamObject(streamObject.Data, streamObject.Path[1])))
                                    {
                                        hasSubChanges = true;
                                    }
                                }

                                if (hasSubChanges)
                                {
                                    OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
                                    hasChanges = true;
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

        public IEnumerable<FirebaseProperty> GetRawPersistableProperties()
        {
            return GetRawProperties(nameof(FirebaseObject)).Select(i => (FirebaseProperty)i);
        }

        public T ParseModel<T>()
            where T : FirebaseObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
