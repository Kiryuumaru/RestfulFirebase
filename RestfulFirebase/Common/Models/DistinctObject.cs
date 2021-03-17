using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class DistinctObject : ObservableObjectHolder.ObservableObject
    {
        #region Properties

        public string Key
        {
            get => GetProperty<string>("key");
            private set => SetProperty(value, "key", nameof(Key));
        }

        #endregion

        #region Initializers

        public DistinctObject()
        {
            Key = Helpers.GenerateSafeUID();
        }

        public DistinctObject(ObservableObjectHolder holder) : base(holder)
        {
            Key = Helpers.GenerateSafeUID();
        }

        public DistinctObject(string key)
        {
            Key = key;
        }

        public DistinctObject(string key, IEnumerable<DistinctProperty> properties) : base(properties)
        {
            Key = key;
        }

        #endregion

        #region Methods

        public T ParseDerived<T>()
            where T : DistinctObject
        {

            return (T)Activator.CreateInstance(typeof(T), Holder);
        }

        #endregion
    }
}
