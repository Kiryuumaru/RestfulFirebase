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

        public bool HasRealtimeWire => RealtimeWire != null;

        public string RealtimeWirePath => RealtimeWire.GetAbsolutePath();

        public FirebaseQuery RealtimeWire
        {
            get => Holder.GetAttribute<FirebaseQuery>(nameof(RealtimeWire), nameof(FirebaseObject)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebaseObject), value);
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

        public void Delete()
        {

        }

        protected override DistinctProperty PropertyFactory<T>(T property)
        {
            return new FirebaseProperty(property);
        }

        protected void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = "",
            Func<T, T, bool> validateValue = null,
            Action<(bool HasChanges, PropertyHolder PropertyHolder)> onInternalSet = null)
        {
            SetProperty(value, key, nameof(FirebaseObject), propertyName, validateValue, internalSet =>
            {
                if (internalSet.HasChanges)
                {
                    var prop = (FirebaseProperty)internalSet.PropertyHolder.Property;
                    prop.Modified = CurrentDateTimeFactory();
                }
                onInternalSet?.Invoke(internalSet);
            });
        }

        protected T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = "",
            Action<(bool HasChanges, PropertyHolder PropertyHolder)> onInternalSet = null)
        {
            return GetProperty(key, nameof(FirebaseObject), defaultValue, propertyName, internalSet =>
            {
                if (internalSet.HasChanges)
                {
                    var prop = (FirebaseProperty)internalSet.PropertyHolder.Property;
                    prop.Modified = CurrentDateTimeFactory();
                }
                onInternalSet?.Invoke(internalSet);
            });
        }

        public void StartRealtime(FirebaseQuery query, bool invokeSetFirst, out Action<StreamEvent> onNext)
        {
            RealtimeWire = query;
            var subRealtimes = new List<(FirebaseProperty Property, Action<StreamEvent> OnNext)>();
            onNext = new Action<StreamEvent>(streamEvent =>
            {
                if (!HasRealtimeWire) throw new Exception("Model is not realtime");
                try
                {
                    if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                    else if (streamEvent.Path.Length == 1)
                    {
                        var data = streamEvent.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamEvent.Data);
                        var props = data.Select(i => (i.Key, i.Value.ToString()));
                        ReplaceRawProperties(props, perItemFollowup =>
                        {
                            if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                            {
                                var childQuery = new ChildQuery(RealtimeWire.App, RealtimeWire, () => firebaseProperty.Key);
                                firebaseProperty.StartRealtime(childQuery, invokeSetFirst, out Action<StreamEvent> childOnNext);
                                subRealtimes.Add((firebaseProperty, childOnNext));
                            }
                        });
                    }
                    else if (streamEvent.Path.Length == 2)
                    {
                        var subRealtime = subRealtimes.FirstOrDefault(i => i.Property.Key == streamEvent.Path[1]);
                        subRealtime.OnNext(streamEvent.Skip(1));
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
