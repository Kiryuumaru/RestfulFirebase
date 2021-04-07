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

        public bool HasRealtimeWire => RealtimeWirePath != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebaseObject)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebaseObject), value);
        }

        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebaseObject)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebaseObject), value);
        }

        public string TypeIdentifier
        {
            get => GetPersistableProperty<string>("_t");
            protected set => SetPersistableProperty(value, "_t");
        }

        public CompressedDateTime Modified
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

        protected virtual CompressedDateTime CurrentDateTimeFactory()
        {
            return new CompressedDateTime(DateTime.UtcNow);
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
                            var props = data.Select(i => (i.Key, i.Value.ToString()));
                            ReplaceRawProperties(props, perItemFollowup =>
                            {
                                if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                {
                                    firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, firebaseProperty.Key);
                                }
                            });
                        }
                        else if (streamEvent.Path.Length == 2)
                        {
                            var props = new List<(string Key, string Data)>()
                            {
                                (streamEvent.Path[1], streamEvent.Data)
                            };
                            UpdateRawProperties(props, perItemFollowup =>
                            {
                                if (perItemFollowup.PropertyHolder.Property is FirebaseProperty firebaseProperty)
                                {
                                    firebaseProperty.RealtimeWirePath = Helpers.CombineUrl(RealtimeWirePath, firebaseProperty.Key);
                                }
                            });
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

        public void Dispose()
        {
            RealtimeSubscription?.Dispose();
        }

        #endregion
    }
}
