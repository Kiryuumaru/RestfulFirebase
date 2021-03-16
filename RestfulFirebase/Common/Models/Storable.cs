using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class Storable : ObservableObject
    {
        #region Properties

        public string Id { get; }

        #endregion

        #region Initializers

        public Storable()
        {
            Id = Helpers.GenerateSafeUID();
        }

        public Storable(string id)
        {
            Id = id;
        }

        public Storable(string id, IEnumerable<ObservableProperty> properties) : base(properties)
        {
            Id = id;
        }

        #endregion

        #region Methods

        public T ParseDerived<T>()
            where T : Storable, new()
        {

            return (T)Activator.CreateInstance(typeof(T), Id, GetRawProperties());
        }

        #endregion
    }
}
