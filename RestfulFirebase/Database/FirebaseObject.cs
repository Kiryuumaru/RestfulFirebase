using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database
{
    public class FirebaseObject : DistinctObject
    {
        public FirebaseObject() : base()
        {

        }

        public FirebaseObject(ObservableObjectHolder holder) : base(holder)
        {

        }

        public FirebaseObject(string key) : base(key)
        {

        }

        public FirebaseObject(string key, IEnumerable<DistinctProperty> properties) : base(key, properties)
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
