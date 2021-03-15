using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Conversions.Additionals;
using RestfulFirebase.Common.Conversions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class Decodable
    {
        public IEnumerable<byte> Bytes { get; private set; }
        public string Data { get => Encoding.Unicode.GetString(Bytes.ToArray()); }

        public Decodable(string data) => Update(data);

        public Decodable(IEnumerable<byte> bytes) => Update(bytes);

        protected void Update(string data)
        {
            Bytes = Encoding.Unicode.GetBytes(data);
        }

        protected void Update(IEnumerable<byte> bytes)
        {
            Bytes = bytes;
        }

        public static Decodable CreateDerived<T>(T value)
        {
            return DataTypeDecoder.GetDecoder<T>().CreateDerived(value);
        }

        public T ParseValue<T>()
        {
            return DataTypeDecoder.GetDecoder<T>().ParseValue(this);
        }
    }
}
