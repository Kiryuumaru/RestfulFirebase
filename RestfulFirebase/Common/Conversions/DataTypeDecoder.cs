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
                decoders.Add(new CompressedDateTimeDecoder());
                decoders.Add(new TimeSpanDecoder());
            }
        }

        public static DataTypeDecoder<T> GetDecoder<T>()
        {
            var conversion = decoders.FirstOrDefault(i => i.Type == typeof(T));
            if (conversion == null) throw new Exception(typeof(T).Name + " data type not supported");
            return (DataTypeDecoder<T>)conversion;
        }

        public static void RegisterDecoder(DataTypeDecoder convertion)
        {
            if (decoders.Any(i => i.Type == convertion.Type)) throw new Exception("Decoder already registered");
            decoders.RemoveAll(i => i.Type == convertion.Type);
            decoders.Add(convertion);
        }

        public abstract Type Type { get; }
    }

    public abstract class DataTypeDecoder<T> : DataTypeDecoder
    {
        #region Properties

        public override Type Type { get => typeof(T); }

        public abstract string Encode(T value);

        public abstract T Decode(string data);

        #endregion
    }
}
