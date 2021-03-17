using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database
{
    public abstract class FirebaseProperty : DistinctProperty
    {
        protected FirebaseProperty(string key, ObservablePropertyHolder holder) : base(key, holder)
        {

        }

        protected FirebaseProperty(string key, string data) : base(key, data)
        {

        }

        public static FirebaseProperty<T> Create<T>(T value, string key)
        {
            var decodable = DataTypeDecoder.GetDecoder<T>().CreateDerived(value);
            return new FirebaseProperty<T>(key, decodable.Holder.Data);
        }
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        public FirebaseProperty(string key, string data) : base(key, data)
        {
            Key = key;
        }

        public T ParseValue()
        {
            return DataTypeDecoder.GetDecoder<T>().ParseValue(this);
        }
    }
}
