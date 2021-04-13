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
            set => SetPersistableProperty<string>(null, "_m");
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

        protected virtual SmallDateTime CurrentDateTimeFactory()
        {
            return new SmallDateTime(DateTime.UtcNow);
        }

        protected override DistinctProperty PropertyFactory<T>(T property)
        {
            var prop = new FirebaseProperty(property);
            if (RealtimeWire != null)
            {
                var childQuery = new ChildQuery(RealtimeWire.Query.App, RealtimeWire.Query, () => prop.Key);
                prop.StartRealtime(childQuery, true);
            }
            return prop;
        }

        protected void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = "",
            Func<T, T, bool> validateValue = null)
        {
            SetProperty(value, key, nameof(FirebaseObject), propertyName, validateValue);
        }

        protected T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = "")
        {
            return GetProperty(key, nameof(FirebaseObject), defaultValue, propertyName);
        }

        public void Delete()
        {
            foreach (var propHolder in PropertyHolders)
            {
                DeleteProperty(propHolder.Property.Key);
            }
        }

        public void StartRealtime(FirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeWire = new RealtimeWire(query, 
                () =>
                {
                    foreach (var prop in GetRawPersistableProperties())
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
                            foreach (var propHolder in PropertyHolders.Where(i => !props.Any(j => j.Key == i.Property.Key)))
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
                            try
                            {
                                bool hasChanges = false;

                                var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(streamObject.Path[1]));

                                if (propHolder == null)
                                {
                                    propHolder = new PropertyHolder()
                                    {
                                        Property = PropertyFactory(DistinctProperty.CreateFromKey(streamObject.Path[1])),
                                        Group = null,
                                        PropertyName = null
                                    };
                                    PropertyHolders.Add(propHolder);
                                    hasChanges = true;
                                }

                                ((FirebaseProperty)propHolder.Property).RealtimeWire.ConsumeStream(new StreamObject(streamObject.Skip(1).Path, streamObject.Data));

                                if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
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
