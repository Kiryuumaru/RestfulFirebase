using RestfulFirebase.Common.Converters.Additionals;
using RestfulFirebase.Common.Converters.Primitives;
using RestfulFirebase.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Converters
{
    public class ConverterHolder
    {
        private Func<object, string> encode;
        private Func<string, object> decode;

        public ConverterHolder(Func<object, string> encode, Func<string, object> decode)
        {
            this.encode = encode;
            this.decode = decode;
        }

        public string Encode(object value) => encode(value);
        public object Decode(string data) => decode(data);
    }

    public class ConverterHolder<T> : ConverterHolder
    {
        public ConverterHolder(Func<T, string> encode, Func<string, T> decode)
            : base(
                  new Func<object, string>(obj => encode((T)obj)),
                  new Func<string, object>(data => decode(data)))
        {

        }

        public string Encode(T value) => base.Encode(value);
        public new T Decode(string data) => (T)base.Decode(data);
    }

    public abstract class DataTypeConverter
    {
        private static readonly List<DataTypeConverter> converters = new List<DataTypeConverter>();
        private static bool isInitialized = false;

        static DataTypeConverter()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                converters.Add(new BoolConverter());
                converters.Add(new ByteConverter());
                converters.Add(new SByteConverter());
                converters.Add(new CharConverter());
                converters.Add(new DecimalConverter());
                converters.Add(new DoubleConverter());
                converters.Add(new FloatConverter());
                converters.Add(new IntConverter());
                converters.Add(new UIntConverter());
                converters.Add(new LongConverter());
                converters.Add(new ULongConverter());
                converters.Add(new ShortConverter());
                converters.Add(new UShortConverter());
                converters.Add(new StringConverter());
                converters.Add(new DateTimeConverter());
                converters.Add(new SmallDateTimeConverter());
                converters.Add(new TimeSpanConverter());
            }
        }

        public static ConverterHolder GetConverter(Type type)
        {
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in converters)
                {
                    if (conv.Type == arrayType)
                    {
                        return new ConverterHolder(
                            values => conv.EncodeEnumerableObject(values),
                            data => conv.DecodeEnumerableObject(data));
                    }
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetGenericArguments()?.Length == 1)
            {
                var genericType = type.GetGenericArguments()[0];
                foreach (var conv in converters)
                {
                    if (conv.Type == genericType)
                    {
                        return new ConverterHolder(
                            values => conv.EncodeEnumerableObject(values),
                            data => conv.DecodeEnumerableObject(data));
                    }
                }
            }
            else
            {
                foreach (var conv in converters)
                {
                    if (conv.Type == type)
                    {
                        var derivedConv = (DataTypeConverter)conv;
                        return new ConverterHolder(
                            conv.EncodeObject,
                            conv.DecodeObject);
                    }
                }
            }
            throw new Exception(type.Name + " data type not supported");
        }

        public static ConverterHolder<T> GetConverter<T>()
        {
            var type = typeof(T);
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in converters)
                {
                    if (conv.Type == arrayType)
                    {
                        return new ConverterHolder<T>(
                            values => conv.EncodeEnumerableObject(values),
                            data => (T)conv.DecodeEnumerableObject(data));
                    }
                }
            }
            else if(typeof(IEnumerable).IsAssignableFrom(typeof(T)) && type.GetGenericArguments()?.Length == 1)
            {
                var genericType = type.GetGenericArguments()[0];
                foreach (var conv in converters)
                {
                    if (conv.Type == genericType)
                    {
                        return new ConverterHolder<T>(
                            values => conv.EncodeEnumerableObject(values),
                            data => (T)conv.DecodeEnumerableObject(data));
                    }
                }
            }
            else
            {
                foreach (var conv in converters)
                {
                    if (conv.Type == type)
                    {
                        var derivedConv = (DataTypeConverter<T>)conv;
                        return new ConverterHolder<T>(
                            derivedConv.Encode,
                            derivedConv.Decode);
                    }
                }
            }
            throw new Exception(typeof(T).Name + " data type not supported");
        }

        public static void RegisterDecoder(DataTypeConverter convertion)
        {
            if (converters.Any(i => i.Type == convertion.Type)) throw new Exception("Decoder already registered");
            converters.RemoveAll(i => i.Type == convertion.Type);
            converters.Add(convertion);
        }

        public abstract Type Type { get; }

        public abstract string EncodeObject(object value);

        public abstract object DecodeObject(string data);

        public abstract string EncodeEnumerableObject(object value);

        public abstract object DecodeEnumerableObject(string data);
    }

    public abstract class DataTypeConverter<T> : DataTypeConverter
    {
        #region Properties

        public override Type Type { get => typeof(T); }

        public abstract string Encode(T value);

        public abstract T Decode(string data);

        public string EncodeEnumerable(IEnumerable<T> values)
        {
            var count = values.Count();
            var encodedValues = new string[count];
            for (int i = 0; i < count; i++)
            {
                encodedValues[i] = Encode(values.ElementAt(i));
            }
            return Helpers.SerializeString(encodedValues);
        }

        public IEnumerable<T> DecodeEnumerable(string data)
        {
            var encodedValues = Helpers.DeserializeString(data);
            var decodedValues = new T[encodedValues.Length];
            for (int i = 0; i < encodedValues.Length; i++)
            {
                decodedValues[i] = Decode(encodedValues[i]);
            }
            return decodedValues;
        }

        public override string EncodeObject(object value)
        {
            return Encode((T)value);
        }

        public override object DecodeObject(string data)
        {
            return Decode(data);
        }

        public override string EncodeEnumerableObject(object value)
        {
            return EncodeEnumerable((IEnumerable<T>)value);
        }

        public override object DecodeEnumerableObject(string data)
        {
            return DecodeEnumerable(data);
        }

        #endregion
    }
}
