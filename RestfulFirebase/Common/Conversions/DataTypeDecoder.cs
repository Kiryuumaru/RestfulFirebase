using RestfulFirebase.Common.Conversions.Additionals;
using RestfulFirebase.Common.Conversions.Primitives;
using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Conversions
{
    public abstract class DataTypeDecoder
    {
        private static readonly List<DataTypeDecoder> decoders = new List<DataTypeDecoder>();
        private static bool isInitialized = false;

        public static DataTypeDecoder<T> GetDecoder<T>()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                decoders.Add(new BoolDecoder());
                decoders.Add(new ByteDecoder());
                decoders.Add(new SByteDecoder());
                decoders.Add(new CharDecoder());
                decoders.Add(new DecimalDecoder());
                decoders.Add(new DoubleDecoder());
                decoders.Add(new FloatDecoder());
                decoders.Add(new IntDecoder());
                decoders.Add(new UIntDecoder());
                decoders.Add(new LongDecoder());
                decoders.Add(new ULongDecoder());
                decoders.Add(new ShortDecoder());
                decoders.Add(new UShortDecoder());
                decoders.Add(new StringDecoder());
                decoders.Add(new DateTimeDecoder());
                decoders.Add(new TimeSpanDecoder());
            }
            var conversion = decoders.FirstOrDefault(i => i.Type == typeof(T));
            if (conversion == null) throw new Exception(typeof(T).Name + " data type not supported");
            return (DataTypeDecoder<T>)conversion;
        }

        public static void RegisterDecoder(DataTypeDecoder convertion)
        {
            if (decoders.Any(i => i.Type == convertion.Type || i.TypeIdentifier == convertion.TypeIdentifier)) throw new Exception("Decoder already registered");
            decoders.RemoveAll(i => i.Type == convertion.Type);
            decoders.Add(convertion);
        }

        public abstract Type Type { get; }
        public abstract string TypeIdentifier { get; }
    }

    public abstract class DataTypeDecoder<T> : DataTypeDecoder
    {
        #region Properties

        public override Type Type { get => typeof(T); }
        protected abstract string ParseValue(T value);
        protected abstract T ParseData(string data);

        public string Parse(T value)
        {
            var data = ParseValue(value);
            var encoded = Helpers.SerializeString(TypeIdentifier, data);
            if (encoded == null) throw new Exception("Data encoded is null.");
            return encoded;
        }

        public T Parse(string data)
        {
            if (data == null) throw new Exception("Data to decode is null.");
            var decoded = Helpers.DeserializeString(data);
            if (decoded == null) throw new Exception("Data decoded is null.");
            if (decoded.Length != 2) throw new Exception("Data length error.");
            if (decoded[0] != TypeIdentifier) throw new Exception("Data decoded type mismatch.");
            return ParseData(decoded[1]);
        }

        #endregion
    }
}
