using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Conversions.Additionals;
using RestfulFirebase.Common.Conversions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class DistinctProperty : ObservablePropertyHolder.ObservableProperty
    {
        public string Key { get; protected set; }

        public DistinctProperty(string key, ObservablePropertyHolder holder) : base(holder)
        {
            Key = key;
        }

        public DistinctProperty(string key, string data) : base(data)
        {
            Key = key;
        }

        public DistinctProperty(string key, IEnumerable<byte> bytes) : base(bytes)
        {
            Key = key;
        }

        public bool Update(DistinctProperty cellModel)
        {
            if (cellModel.Key.Equals(Key))
            {
                Update(cellModel.Holder.Data);
                return true;
            }
            return false;
        }

        public static DistinctProperty CreateDerived<T>(T value, string key)
        {
            var decodable = DataTypeDecoder.GetDecoder<T>().CreateDerived(value);
            return new DistinctProperty(key, decodable.Holder.Data);
        }
    }
}
