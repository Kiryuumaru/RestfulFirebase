﻿using RestfulFirebase.Common.Serializers.Additionals;
using RestfulFirebase.Common.Serializers.Primitives;
using RestfulFirebase.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Serializers
{
    public class SerializerHolder
    {
        private Func<object, string> serialize;
        private Func<string, object, object> deserialize;

        public SerializerHolder(Func<object, string> serialize, Func<string, object, object> deserialize)
        {
            this.serialize = serialize;
            this.deserialize = deserialize;
        }

        public string Serialize(object value) => serialize(value);
        public object Deserialize(string data, object defaultValue = default) => deserialize(data, defaultValue);
    }

    public class SerializerHolder<T> : SerializerHolder
    {
        public SerializerHolder(Func<T, string> serialize, Func<string, T, T> deserialize)
            : base(
                  new Func<object, string>(obj => serialize((T)obj)),
                  new Func<string, object, object>((data, defaultValue) => deserialize(data, (T)defaultValue)))
        {

        }

        public string Serialize(T value) => base.Serialize(value);
        public T Deserialize(string data, T defaultValue = default) => (T)base.Deserialize(data, defaultValue);
    }

    public abstract class Serializer
    {
        private static readonly List<Serializer> serializers = new List<Serializer>();
        private static bool isInitialized = false;

        static Serializer()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                serializers.Add(new BoolSerializer());
                serializers.Add(new ByteSerializer());
                serializers.Add(new SByteSerializer());
                serializers.Add(new CharSerializer());
                serializers.Add(new DecimalSerializer());
                serializers.Add(new DoubleSerializer());
                serializers.Add(new FloatSerializer());
                serializers.Add(new IntSerializer());
                serializers.Add(new UIntSerializer());
                serializers.Add(new LongSerializer());
                serializers.Add(new ULongSerializer());
                serializers.Add(new ShortSerializer());
                serializers.Add(new UShortSerializer());
                serializers.Add(new StringSerializer());
                serializers.Add(new DateTimeSerializer());
                serializers.Add(new SmallDateTimeSerializer());
                serializers.Add(new TimeSpanSerializer());
            }
        }

        public static SerializerHolder GetSerializer(Type type)
        {
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in serializers)
                {
                    if (conv.Type == arrayType)
                    {
                        return new SerializerHolder(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetGenericArguments()?.Length == 1)
            {
                var genericType = type.GetGenericArguments()[0];
                foreach (var conv in serializers)
                {
                    if (conv.Type == genericType)
                    {
                        return new SerializerHolder(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else
            {
                foreach (var conv in serializers)
                {
                    if (conv.Type == type)
                    {
                        var derivedConv = (Serializer)conv;
                        return new SerializerHolder(
                            conv.SerializeObject,
                            conv.DeserializeObject);
                    }
                }
            }
            throw new Exception(type.Name + " data type not supported");
        }

        public static SerializerHolder<T> GetSerializer<T>()
        {
            var type = typeof(T);
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in serializers)
                {
                    if (conv.Type == arrayType)
                    {
                        return new SerializerHolder<T>(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => (T)conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else if(typeof(IEnumerable).IsAssignableFrom(typeof(T)) && type.GetGenericArguments()?.Length == 1)
            {
                var genericType = type.GetGenericArguments()[0];
                foreach (var conv in serializers)
                {
                    if (conv.Type == genericType)
                    {
                        return new SerializerHolder<T>(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) => (T)conv.DeserializeEnumerableObject(data, defaultValue));
                    }
                }
            }
            else
            {
                foreach (var conv in serializers)
                {
                    if (conv.Type == type)
                    {
                        var derivedConv = (Serializer<T>)conv;
                        return new SerializerHolder<T>(
                            derivedConv.Serialize,
                            derivedConv.Deserialize);
                    }
                }
            }
            throw new Exception(typeof(T).Name + " data type not supported");
        }

        public static string Serialize<T>(T value)
        {
            return GetSerializer<T>().Serialize(value);
        }

        public static T Deserialize<T>(string data, T defaultValue = default)
        {
            return GetSerializer<T>().Deserialize(data, defaultValue);
        }

        public static void Register(Serializer serializer)
        {
            if (serializers.Any(i => i.Type == serializer.Type)) throw new Exception("Serializer already registered");
            serializers.RemoveAll(i => i.Type == serializer.Type);
            serializers.Add(serializer);
        }

        public abstract Type Type { get; }

        public abstract string SerializeObject(object value);

        public abstract object DeserializeObject(string data, object defaultValue = default);

        public abstract string SerializeEnumerableObject(object value);

        public abstract object DeserializeEnumerableObject(string data, object defaultValue = default);
    }

    public abstract class Serializer<T> : Serializer
    {
        #region Properties

        public override Type Type { get => typeof(T); }

        public abstract string Serialize(T value);

        public abstract T Deserialize(string data, T defaultValue = default);

        public string SerializeEnumerable(IEnumerable<T> values)
        {
            if (values == null) return null;
            var count = values.Count();
            var encodedValues = new string[count];
            for (int i = 0; i < count; i++)
            {
                encodedValues[i] = Serialize(values.ElementAt(i));
            }
            return Helpers.SerializeString(encodedValues);
        }

        public IEnumerable<T> DeserializeEnumerable(string data, IEnumerable<T> defaultValue = default)
        {
            var encodedValues = Helpers.DeserializeString(data);
            if (encodedValues == null) return defaultValue;
            var decodedValues = new T[encodedValues.Length];
            for (int i = 0; i < encodedValues.Length; i++)
            {
                decodedValues[i] = Deserialize(encodedValues[i]);
            }
            return decodedValues;
        }

        public override string SerializeObject(object value)
        {
            return Serialize((T)value);
        }

        public override object DeserializeObject(string data, object defaultValue = default)
        {
            return Deserialize(data, (T)defaultValue);
        }

        public override string SerializeEnumerableObject(object value)
        {
            return SerializeEnumerable((IEnumerable<T>)value);
        }

        public override object DeserializeEnumerableObject(string data, object defaultValue = default)
        {
            return DeserializeEnumerable(data, (IEnumerable<T>)defaultValue);
        }

        #endregion
    }
}
