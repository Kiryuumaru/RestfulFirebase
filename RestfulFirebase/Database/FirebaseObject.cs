using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database
{
    public class FirebaseObject : DistinctObject
    {
        protected FirebaseObject()
        {

        }

        protected FirebaseObject(ObservableObjectHolder holder) : base(holder)
        {

        }

        protected FirebaseObject(string key) : base(key)
        {

        }

        protected FirebaseObject(string key, IEnumerable<DistinctProperty> properties) : base(key, properties)
        {

        }

        public void SetPersistableProperty<T>(T value, string key, [CallerMemberName] string propertyName = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            SetProperty(value, key, propertyName, "persistable", onChanged, validateValue);
        }

        public IEnumerable<DistinctProperty> GetPersistableRawProperties()
        {
            return GetRawProperties("persistable");
        }

        public T Parse<T>()
            where T : FirebaseObject
        {
            return (T)Activator.CreateInstance(typeof(T), Holder);
        }
    }
}
