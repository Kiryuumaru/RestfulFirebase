using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database
{
    public class FirebaseObject : DistinctObject
    {
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

        public void SetPersistableProperty<T>(T value, string key, [CallerMemberName] string propertyName = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            SetProperty(value, key, nameof(FirebaseObject), propertyName, onChanged, validateValue);
        }

        public T GetPersistableProperty<T>(string key, [CallerMemberName] string propertyName = "")
        {
            return GetProperty<T>(key, nameof(FirebaseObject), propertyName);
        }

        public IEnumerable<DistinctProperty> GetPersistableRawProperties()
        {
            return GetRawProperties(nameof(FirebaseObject));
        }

        #endregion
    }
}
