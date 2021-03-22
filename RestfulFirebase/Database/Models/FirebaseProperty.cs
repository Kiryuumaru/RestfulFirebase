using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public abstract class FirebaseProperty : DistinctProperty, IDisposable
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeSubscription != null;
        internal IDisposable RealtimeSubscription { get; set; }

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

        public FirebaseProperty(AttributeHolder holder) : base(holder)
        {

        }

        public void Dispose()
        {
            RealtimeSubscription?.Dispose();
        }

        #endregion
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Initializers

        public FirebaseProperty(AttributeHolder holder) : base(holder)
        {

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

        public T ParseValue()
        {
            return ParseValue<T>();
        }

        #endregion
    }
}
