using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public abstract class FirebaseProperty : DistinctProperty, IDisposable
    {
        #region Properties

        public event EventHandler OnDisposing;

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
            OnDisposing?.Invoke(this, new EventArgs());
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

        public T ParseValue()
        {
            return ParseValue<T>();
        }

        #endregion
    }
}
