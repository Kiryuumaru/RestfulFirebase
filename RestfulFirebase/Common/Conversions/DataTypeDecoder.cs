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

        static DataTypeDecoder()
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
        }

        protected static string SerializeData(string typeIdentifier, string data)
        {
            var encoded = Helpers.SerializeString(typeIdentifier, data);
            if (encoded == null) throw new Exception("Data encoded is null.");
            return encoded;
        }

        protected static string[] DeserializeData(string data)
        {
            if (data == null) throw new Exception("Data to decode is null.");
            var decoded = Helpers.DeserializeString(data);
            if (decoded == null) throw new Exception("Data decoded is null.");
            if (decoded.Length != 2) throw new Exception("Data length error.");
            return decoded;
        }

        public static Type GetDataType(string data)
        {
            var deserialized = DeserializeData(data);
            var conversion = decoders.FirstOrDefault(i => i.TypeIdentifier == deserialized[0]);
            if (conversion == null) throw new Exception(deserialized[0] + " data type not supported");
            return conversion.Type;
        }

        public static DataTypeDecoder GetDecoder(Type type)
        {
            var conversion = decoders.FirstOrDefault(i => i.Type == type);
            if (conversion == null) throw new Exception(type.Name + " data type not supported");
            return conversion;
        }

        public static DataTypeDecoder<T> GetDecoder<T>()
        {
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

        protected abstract string EncodeValueAsObject(object value);
        protected abstract object DecodeDataAsObject(string data);

        public string EncodeAsObject(object value)
        {
            var data = EncodeValueAsObject(value);
            return SerializeData(TypeIdentifier, data);
        }

        public object DecodeAsObject(string data)
        {
            var deserialized = DeserializeData(data);
            return DecodeDataAsObject(deserialized[1]);
        }
    }

    public abstract class DataTypeDecoder<T> : DataTypeDecoder
    {
        #region Properties

        public override Type Type { get => typeof(T); }

        protected abstract string EncodeValue(T value);

        protected abstract T DecodeData(string data);

        protected override string EncodeValueAsObject(object value) => EncodeValue((T)value);

        protected override object DecodeDataAsObject(string data) => DecodeData(data);

        public string Encode(T value)
        {
            var data = EncodeValue(value);
            return SerializeData(TypeIdentifier, data);
        }

        public T Decode(string data)
        {
            var deserialized = DeserializeData(data);
            return DecodeData(deserialized[1]);
        }

        #endregion
    }
}
