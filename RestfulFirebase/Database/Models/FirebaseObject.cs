using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObject : DistinctObject, IDisposable
    {
        #region Properties

        public event EventHandler OnDisposing;

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

        public FirebaseObject(AttributeHolder holder) : base(holder)
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

        internal void ConsumeStream(StreamEvent streamEvent)
        {
            if (streamEvent.Path.Length != 0)
            {
                if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key Mismatch");
            }

        }

        public IEnumerable<DistinctProperty> GetRawPersistableProperties()
        {
            return GetRawProperties(nameof(FirebaseObject));
        }

        public void Dispose()
        {
            OnDisposing?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
