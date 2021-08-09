using RestfulFirebase.Exceptions;
using RestfulFirebase.Extensions;
using RestfulFirebase.Serializers.Additionals;
using RestfulFirebase.Serializers.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Serializers
{
    /// <summary>
    /// Provides implementation for value serializer and deserializer.
    /// </summary>
    public abstract class Serializer
    {
        private static readonly List<Serializer> serializers = new List<Serializer>()
        {
            new BoolSerializer(),
            new ByteSerializer(),
            new SByteSerializer(),
            new CharSerializer(),
            new DecimalSerializer(),
            new DoubleSerializer(),
            new FloatSerializer(),
            new IntSerializer(),
            new UIntSerializer(),
            new LongSerializer(),
            new ULongSerializer(),
            new ShortSerializer(),
            new UShortSerializer(),
            new StringSerializer(),
            new DateTimeSerializer(),
            new TimeSpanSerializer()
        };

        /// <summary>
        /// Gets the serializer for the provided type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// The type of the serializer to get.
        /// </param>
        /// <returns>
        /// The serializer proxy of the provided type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws if serializer is not supported.
        /// </exception>
        public static SerializerProxy GetSerializer(Type type)
        {
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in serializers)
                {
                    if (conv.Type == arrayType)
                    {
                        return new SerializerProxy(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return conv.DeserializeEnumerableObject(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
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
                        return new SerializerProxy(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return conv.DeserializeEnumerableObject(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
                    }
                }
            }
            else if (Nullable.GetUnderlyingType(type) != null)
            {
                var nullableType = Nullable.GetUnderlyingType(type);
                foreach (var conv in serializers)
                {
                    if (conv.Type == nullableType)
                    {
                        return new SerializerProxy(
                            value => conv.SerializeNullableObject(value),
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return conv.DeserializeNullableObject(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
                    }
                }
            }
            else
            {
                foreach (var conv in serializers)
                {
                    if (conv.Type == type)
                    {
                        return new SerializerProxy(
                            conv.SerializeObject,
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return conv.DeserializeObject(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
                    }
                }
            }
            throw new SerializerNotSupportedException(type);
        }

        /// <summary>
        /// Gets the serializer for the provided type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Underlying type of the serializer.
        /// </typeparam>
        /// <returns>
        /// The serializer proxy of the provided type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws if serializer is not supported.
        /// </exception>
        public static SerializerProxy<T> GetSerializer<T>()
        {
            var type = typeof(T);
            if (type.IsArray)
            {
                var arrayType = type.GetElementType();
                foreach (var conv in serializers)
                {
                    if (conv.Type == arrayType)
                    {
                        return new SerializerProxy<T>(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return (T)conv.DeserializeEnumerableObject(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
                    }
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && type.GetGenericArguments()?.Length == 1)
            {
                var genericType = type.GetGenericArguments()[0];
                foreach (var conv in serializers)
                {
                    if (conv.Type == genericType)
                    {
                        return new SerializerProxy<T>(
                            values => conv.SerializeEnumerableObject(values),
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return (T)conv.DeserializeEnumerableObject(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
                    }
                }
            }
            else if (Nullable.GetUnderlyingType(type) != null)
            {
                var nullableType = Nullable.GetUnderlyingType(type);
                foreach (var conv in serializers)
                {
                    if (conv.Type == nullableType)
                    {
                        return new SerializerProxy<T>(
                            value => conv.SerializeNullableObject(value),
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return (T)conv.DeserializeNullableObject(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
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
                        return new SerializerProxy<T>(
                            derivedConv.Serialize,
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    return derivedConv.Deserialize(data);
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
                    }
                }
            }
            throw new SerializerNotSupportedException(type);
        }

        /// <summary>
        /// Serialize the provided <paramref name="value"/> with the specified type <paramref name="type"/>.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <param name="type">
        /// The type of the value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        public static string Serialize(object value, Type type)
        {
            return GetSerializer(type).Serialize(value);
        }

        /// <summary>
        /// Serialize the provided <paramref name="value"/> with the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value to serialize.
        /// </typeparam>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        public static string Serialize<T>(T value)
        {
            return GetSerializer<T>().Serialize(value);
        }

        /// <summary>
        /// Deserialize the provided <paramref name="data"/> with the specified type <paramref name="type"/>.
        /// </summary>
        /// <param name="data">
        /// The data to deserialize.
        /// </param>
        /// <param name="type">
        /// The type of the value to deserialize.
        /// </param>
        /// <param name="defaultValue">
        /// The default value return if the serializer throws an exception.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public static object Deserialize(string data, Type type, object defaultValue = default)
        {
            return GetSerializer(type).Deserialize(data, defaultValue);
        }

        /// <summary>
        /// Deserialize the provided <paramref name="data"/> with the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value to deserialize.
        /// </typeparam>
        /// <param name="data">
        /// The data to deserialize.
        /// </param>
        /// <param name="defaultValue">
        /// The default value return if the serializer throws an exception.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public static T Deserialize<T>(string data, T defaultValue = default)
        {
            return GetSerializer<T>().Deserialize(data, defaultValue);
        }

        /// <summary>
        /// Check if the type can be serialize.
        /// </summary>
        /// <param name="type">
        /// The type to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="type"/> can be serialize; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanSerialize(Type type)
        {
            try
            {
                _ = GetSerializer(type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the value can be serialize.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the value to check.
        /// </typeparam>
        /// <param name="value">
        /// The value to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="value"/> can be serialize; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanSerialize<T>(T value)
        {
            try
            {
                _ = GetSerializer<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the type can be serialize.
        /// </summary>
        /// <typeparam name="T">
        /// The type to check.
        /// </typeparam>
        /// <returns>
        /// <c>true</c> if the <typeparamref name="T"/> can be serialize; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanSerialize<T>()
        {
            try
            {
                _ = GetSerializer<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Globally register a <see cref="Serializer"/>.
        /// </summary>
        /// <param name="serializer">
        /// The <see cref="Serializer"/> to register.
        /// </param>
        public static void Register(Serializer serializer)
        {
            serializers.RemoveAll(i => i.Type == serializer.Type);
            serializers.Add(serializer);
        }

        /// <summary>
        /// The type of the serializer.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Serialize the provided <paramref name="value"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        public abstract string SerializeObject(object value);

        /// <summary>
        /// Deserialize the provided <paramref name="data"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="data">
        /// The data to deserialize.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public abstract object DeserializeObject(string data);

        /// <summary>
        /// Serialize the provided <see cref="IEnumerable"/> <paramref name="value"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data
        /// </returns>
        public abstract string SerializeEnumerableObject(object value);

        /// <summary>
        /// Deserialize the provided <see cref="IEnumerable"/> <paramref name="data"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="data">
        /// The data to deserialize.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public abstract object DeserializeEnumerableObject(string data);

        /// <summary>
        /// Serialize the provided nullable <paramref name="value"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        public abstract string SerializeNullableObject(object value);

        /// <summary>
        /// Deserialize the provided nullable <paramref name="data"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="data">
        /// The data to deserialize.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public abstract object DeserializeNullableObject(string data);
    }

    /// <summary>
    /// Provides implementation for value serializer and deserializer.
    /// </summary>
    /// <typeparam name="T">
    /// The specified type of the serializer.
    /// </typeparam>
    public abstract class Serializer<T> : Serializer
    {
        #region Properties

        /// <inheritdoc/>
        public override Type Type { get => typeof(T); }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public abstract string Serialize(T value);

        /// <inheritdoc/>
        public abstract T Deserialize(string data);

        /// <inheritdoc/>
        public string SerializeEnumerable(IEnumerable<T> values)
        {
            if (values == null) return null;
            var count = values.Count();
            var encodedValues = new string[count];
            for (int i = 0; i < count; i++)
            {
                encodedValues[i] = Serialize(values.ElementAt(i));
            }
            return Utils.SerializeString(encodedValues);
        }

        /// <inheritdoc/>
        public IEnumerable<T> DeserializeEnumerable(string data, IEnumerable<T> defaultValue = default)
        {
            var encodedValues = Utils.DeserializeString(data);
            if (encodedValues == null) return defaultValue;
            var decodedValues = new T[encodedValues.Length];
            for (int i = 0; i < encodedValues.Length; i++)
            {
                decodedValues[i] = Deserialize(encodedValues[i]);
            }
            return decodedValues;
        }

        /// <inheritdoc/>
        public override string SerializeObject(object value)
        {
            return Serialize((T)value);
        }

        /// <inheritdoc/>
        public override object DeserializeObject(string data)
        {
            return Deserialize(data);
        }

        /// <inheritdoc/>
        public override string SerializeEnumerableObject(object value)
        {
            return SerializeEnumerable((IEnumerable<T>)value);
        }

        /// <inheritdoc/>
        public override object DeserializeEnumerableObject(string data)
        {
            return DeserializeEnumerable(data);
        }

        /// <inheritdoc/>
        public override string SerializeNullableObject(object value)
        {
            if (value == null) return null;
            return Serialize((T)value);
        }

        /// <inheritdoc/>
        public override object DeserializeNullableObject(string data)
        {
            if (data == null) return null;
            return Deserialize(data);
        }

        #endregion
    }
}
