using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public abstract class FirebaseProperty : DistinctProperty, IDisposable
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeSubscription != null;
        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebaseProperty)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebaseProperty), value);
        }
        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebaseProperty)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebaseProperty), value);
        }

        #endregion

        #region Initializers

        public static new FirebaseProperty<T> CreateFromKeyAndValue<T>(string key, T value)
        {
            return new FirebaseProperty<T>(DistinctProperty.CreateFromKeyAndValue(key, value));
        }

        public static FirebaseProperty<T> CreateFromKeyAndData<T>(string key, string data)
        {
            return new FirebaseProperty<T>(CreateFromKeyAndData(key, data));
        }

        public FirebaseProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        public void Dispose()
        {
            RealtimeSubscription?.Dispose();
        }

        #endregion

        #region Methods

        internal void ConsumePersistableStream(StreamEvent streamEvent)
        {
            if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
            else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
            else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
            else if (streamEvent.Path.Length == 1)
            {
                if (streamEvent.Data == null) Empty();
                else Update(streamEvent.Data);
            }
        }

        #endregion
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Initializers

        public FirebaseProperty(IAttributed attributed) : base(attributed)
        {

        }

        #endregion

        #region Methods

        public T ParseValue()
        {
            return ParseValue<T>();
        }

        #endregion
    }
}
