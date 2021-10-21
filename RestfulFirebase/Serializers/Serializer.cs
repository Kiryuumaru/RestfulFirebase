using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using RestfulFirebase.Serializers.Additionals;
using RestfulFirebase.Serializers.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            return GetSerializerInternal(type, () => new SerializerProxy());
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
            return GetSerializerInternal(typeof(T), () => new SerializerProxy<T>());
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
        /// <exception cref="SerializerNotSupportedException">
        /// Throws if serializer is not supported.
        /// </exception>
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
        /// <exception cref="SerializerNotSupportedException">
        /// Throws if serializer is not supported.
        /// </exception>
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
        /// <exception cref="SerializerNotSupportedException">
        /// Throws if serializer is not supported.
        /// </exception>
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
        /// <exception cref="SerializerNotSupportedException">
        /// Throws if serializer is not supported.
        /// </exception>
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
        /// <param name="defaultValue">
        /// The default value return if operation failed.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public abstract object DeserializeObject(string data, object defaultValue = default);

        /// <summary>
        /// Serialize the provided <paramref name="value"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data
        /// </returns>
        public abstract string SerializeEnumerableObject(object value);

        /// <summary>
        /// Deserialize the provided <paramref name="data"/> with the specified type <see cref="Type"/>.
        /// </summary>
        /// <param name="data">
        /// The data to deserialize.
        /// </param>
        /// <param name="defaultValue">
        /// The default value return if operation failed.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public abstract object DeserializeEnumerableObject(string data, object defaultValue = default);

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
        /// <param name="defaultValue">
        /// The default value return if operation failed.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public abstract object DeserializeNullableObject(string data, object defaultValue = default);

        internal static T GetSerializerInternal<T>(
            Type type,
            Func<T> initializer,
            Func<Serializer, object, string> onSet = null,
            Func<Serializer, string, object, object> onGet = null)
            where T : SerializerProxy
        {
            var proxy = initializer();
            foreach (var conv in serializers)
            {
                if (conv.Type == type)
                {
                    proxy.Set(
                        value =>
                        {
                            return onSet == null ? conv.SerializeObject(value) : onSet.Invoke(conv, value);
                        },
                        (data, defaultValue) =>
                        {
                            if (onGet == null)
                            {
                                return conv.DeserializeObject(data, defaultValue);
                            }
                            else
                            {
                                return onGet.Invoke(conv, data, defaultValue);
                            }
                        });
                    return proxy;
                }
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                return GetSerializerInternal(nullableType, initializer,
                    (conv, value) =>
                    {
                        return value == null ? null : conv.SerializeObject(value);
                    },
                    (conv, data, defaultValue) =>
                    {
                        return conv.DeserializeObject(data, defaultValue);
                    });
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsArray)
                {
                    var arrayType = type.GetElementType();
                    return GetSerializerInternal(arrayType, initializer,
                        (conv, value) =>
                        {
                            return conv.SerializeEnumerableObject(value);
                        },
                        (conv, data, defaultValue) =>
                        {
                            return conv.DeserializeEnumerableObject(data, defaultValue);
                        });
                }

                ConstructorInfo constructor = type.GetConstructors()
                    .FirstOrDefault(i =>
                    {
                        if (!i.IsPublic)
                        {
                            return false;
                        }
                        var parameters = i.GetParameters();
                        if (parameters == null)
                        {
                            return false;
                        }
                        if (parameters.Length != 1)
                        {
                            return false;
                        }
                        if (!typeof(IEnumerable).IsAssignableFrom(parameters[0].ParameterType))
                        {
                            return false;
                        }
                        return true;
                    });

                if (constructor != null)
                {
                    var parameterType = constructor.GetParameters()[0].ParameterType;
                    var genericArguments = parameterType.GetGenericArguments();
                    if (genericArguments.Length == 1)
                    {
                        var genericType = genericArguments[0];
                        if (genericType.IsGenericType)
                        {
                            var keyValuePairType = typeof(KeyValuePair<,>);
                            if (typeof(IDictionary).IsAssignableFrom(type) && genericType.GetGenericTypeDefinition() == keyValuePairType)
                            {
                                var pairGenericArgs = genericType.GetGenericArguments();
                                var keyConv = GetSerializer(pairGenericArgs[0]);
                                var valueConv = GetSerializer(pairGenericArgs[1]);
                                proxy.Set(
                                    pairs =>
                                    {
                                        var blobArray = Array.Empty<string>();
                                        foreach (var pair in (IEnumerable)pairs)
                                        {
                                            var key = genericType.GetProperty("Key").GetValue(pair, new object[0]);
                                            var value = genericType.GetProperty("Value").GetValue(pair, new object[0]);
                                            blobArray = BlobUtilities.SetValue(blobArray, keyConv.Serialize(key), valueConv.Serialize(value));
                                        }
                                        return StringUtilities.Serialize(blobArray);
                                    },
                                    (data, defaultValue) =>
                                    {
                                        try
                                        {
                                            Dictionary<string, string> deserialized = BlobUtilities.Convert(data);
                                            var valuesType = typeof(List<>).MakeGenericType(genericType);
                                            var values = Activator.CreateInstance(valuesType);
                                            foreach (var pair in deserialized)
                                            {
                                                valuesType.GetMethod("Add").Invoke(values, new object[] {
                                                    Activator.CreateInstance(genericType,
                                                        keyConv.Deserialize(pair.Key),
                                                        valueConv.Deserialize(pair.Value)) });
                                            }
                                            try
                                            {
                                                return constructor.Invoke(new object[] { values });
                                            }
                                            catch
                                            {
                                                throw new SerializerNotSupportedException(type);
                                            }
                                        }
                                        catch (SerializerException)
                                        {
                                            throw;
                                        }
                                        catch
                                        {
                                            return defaultValue;
                                        }
                                    });
                                return proxy;
                            }
                        }
                        else
                        {
                            return GetSerializerInternal(genericType, initializer,
                                (conv, value) =>
                                {
                                    return conv.SerializeEnumerableObject(value);
                                },
                                (conv, data, defaultValue) =>
                                {
                                    return constructor.Invoke(new object[] { conv.DeserializeEnumerableObject(data, defaultValue) });
                                });
                        }
                    }
                    else if (genericArguments.Length == 0)
                    {
                        proxy.Set(
                            values =>
                            {
                                var serialized = new List<string>();
                                foreach (var value in (IEnumerable)values)
                                {
                                    string serializedType = value.GetType().FullName;
                                    string serializedValue = Serializer.Serialize(value, value.GetType());
                                    serialized.Add(StringUtilities.Serialize(serializedType, serializedValue));
                                }
                                return StringUtilities.Serialize(serialized.ToArray());
                            },
                            (data, defaultValue) =>
                            {
                                try
                                {
                                    string[] deserialized = StringUtilities.Deserialize(data);
                                    var values = new List<object>();
                                    foreach (var value in deserialized)
                                    {
                                        try
                                        {
                                            string[] deserializedPair = StringUtilities.Deserialize(value);
                                            if (deserializedPair?.Length == 2)
                                            {
                                                string serializedType = deserializedPair[0];
                                                string serializedValue = deserializedPair[1];
                                                object deserializedValue = null;
                                                bool hasDeserializer = false;
                                                foreach (var conv in serializers)
                                                {
                                                    if (conv.Type.FullName == serializedType)
                                                    {
                                                        deserializedValue = conv.DeserializeObject(serializedValue);
                                                        hasDeserializer = true;
                                                        break;
                                                    }
                                                }
                                                if (!hasDeserializer)
                                                {
                                                    throw new SerializerNotSupportedException(serializedType);
                                                }
                                                values.Add(deserializedValue);
                                                continue;
                                            }
                                        }
                                        catch (SerializerException)
                                        {
                                            throw;
                                        }
                                        catch { }
                                        values.Add(null);
                                    }
                                    try
                                    {
                                        return constructor.Invoke(new object[] { values });
                                    }
                                    catch
                                    {
                                        throw new SerializerNotSupportedException(type);
                                    }
                                }
                                catch (SerializerException)
                                {
                                    throw;
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            });
                        return proxy;
                    }
                }
            }
            throw new SerializerNotSupportedException(type);
        }
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
        public abstract T Deserialize(string data, T defaultValue = default);

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
            return StringUtilities.Serialize(encodedValues);
        }

        /// <inheritdoc/>
        public IEnumerable<T> DeserializeEnumerable(string data, IEnumerable<T> defaultValue = default)
        {
            string[] encodedValues;
            try
            {
                encodedValues = StringUtilities.Deserialize(data);
            }
            catch
            {
                return defaultValue;
            }
            if (encodedValues == null)
            {
                return defaultValue;
            }
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
        public override object DeserializeObject(string data, object defaultValue = default)
        {
            if (defaultValue is T value)
            {
                return Deserialize(data, value);
            }
            else
            {
                return Deserialize(data);
            }
        }

        /// <inheritdoc/>
        public override string SerializeEnumerableObject(object value)
        {
            return SerializeEnumerable((IEnumerable<T>)value);
        }

        /// <inheritdoc/>
        public override object DeserializeEnumerableObject(string data, object defaultValue = default)
        {
            if (defaultValue is IEnumerable<T> value)
            {
                return DeserializeEnumerable(data, value);
            }
            else
            {
                return DeserializeEnumerable(data);
            }
        }

        /// <inheritdoc/>
        public override string SerializeNullableObject(object value)
        {
            if (value == null) return null;
            return Serialize((T)value);
        }

        /// <inheritdoc/>
        public override object DeserializeNullableObject(string data, object defaultValue = default)
        {
            if (data == null) return null;
            if (defaultValue is T value)
            {
                return Deserialize(data, value);
            }
            else
            {
                return Deserialize(data);
            }
        }

        #endregion
    }
}
