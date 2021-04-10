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

        private bool realtimeWireInvokeSetFirst;

        public bool HasRealtimeWire => RealtimeWire != null;

        public string RealtimeWirePath => RealtimeWire?.GetAbsolutePath();

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
            var prop = new FirebaseProperty(property);
            if (HasRealtimeWire)
            {
                var childQuery = new ChildQuery(RealtimeWire.App, RealtimeWire, () => prop.Key);
                prop.StartRealtime(childQuery, false);
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

        public void StartRealtime(FirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeWire = query;
            realtimeWireInvokeSetFirst = invokeSetFirst;
            foreach (var prop in GetRawPersistableProperties())
            {
                var childQuery = new ChildQuery(RealtimeWire.App, RealtimeWire, () => prop.Key);
                prop.StartRealtime(childQuery, realtimeWireInvokeSetFirst);
            }
        }

        public void ConsumeStream(StreamObject streamObject)
        {
            if (!HasRealtimeWire) throw new Exception("Model is not realtime");
            try
            {
                if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                else if (streamObject.Path.Length == 1)
                {
                    var data = streamObject.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamObject.Data);
                    var props = data.Select(i => (i.Key, i.Value.ToString()));
                    ReplaceBlobs(props);
                }
                else if (streamObject.Path.Length == 2)
                {
                    var props = new List<(string Key, string Data)>()
                    {
                        (streamObject.Path[1], streamObject.Data)
                    };
                    UpdateBlobs(props);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
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
