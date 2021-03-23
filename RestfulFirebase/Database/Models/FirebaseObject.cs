using Newtonsoft.Json;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObject : DistinctObject, IRealtimeModel
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeSubscription != null;

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

        public static new FirebaseObject CreateFromKeyAndProperties(string key, IEnumerable<DistinctProperty> properties)
        {
            return new FirebaseObject(DistinctObject.CreateFromKeyAndProperties(key, properties));
        }

        public FirebaseObject(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods

        protected void SetPersistableProperty<T>(T value, string key, [CallerMemberName] string propertyName = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            SetProperty(value, key, nameof(FirebaseObject), propertyName, onChanged, validateValue);
        }

        protected T GetPersistableProperty<T>(string key, T defaultValue = default, [CallerMemberName] string propertyName = "")
        {
            return GetProperty(key, nameof(FirebaseObject), defaultValue, propertyName);
        }

        internal void ConsumePersistableStream(StreamEvent streamEvent)
        {
            if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
            else if(streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
            else if(streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
            else if(streamEvent.Path.Length == 1)
            {
                foreach (var key in GetRawPersistableProperties().Select(i => i.Key))
                {
                    DeleteProperty(key);
                }
                if (streamEvent.Data != null)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(streamEvent.Data);
                    var props = data.Select(i => DistinctProperty.CreateFromKeyAndData(i.Key, i.Value));
                    PatchRawProperties(props);
                }
            }
            else if (streamEvent.Path.Length == 2)
            {
                var props = new List<DistinctProperty>()
                {
                    DistinctProperty.CreateFromKeyAndData(streamEvent.Path[1], streamEvent.Data)
                };
                PatchRawProperties(props);
            }
        }

        public IEnumerable<DistinctProperty> GetRawPersistableProperties()
        {
            return GetRawProperties(nameof(FirebaseObject));
        }

        public void Dispose()
        {
            RealtimeSubscription?.Dispose();
        }

        #endregion
    }
}
