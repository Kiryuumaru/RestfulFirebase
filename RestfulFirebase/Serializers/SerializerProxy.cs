using System;

namespace RestfulFirebase.Serializers
{
    /// <summary>
    /// Provides implementation holder for serializer and deserializer.
    /// </summary>
    public class SerializerProxy
    {
        private Func<object, string> serialize;
        private Func<string, object, object> deserialize;

        internal SerializerProxy()
        { 
        }

        /// <summary>
        /// Object serializer implementation proxy.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        public string Serialize(object value) => serialize(value);

        /// <summary>
        /// Data deserializer implementation proxy.
        /// </summary>
        /// <param name="data">
        /// Data to deserialized
        /// </param>
        /// <param name="defaultValue">
        /// The default value returned if deserialize throws an exception.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public object Deserialize(string data, object defaultValue = default) => deserialize(data, defaultValue);

        internal void Set(Func<object, string> serialize, Func<string, object, object> deserialize)
        {
            this.serialize = serialize;
            this.deserialize = deserialize;
        }
    }

    /// <summary>
    /// Provides implementation holder for serializer and deserializer.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the value to serialize and deserialize.
    /// </typeparam>
    public class SerializerProxy<T> : SerializerProxy
    {
        internal SerializerProxy()
        {
        }

        /// <summary>
        /// Object serializer implementation proxy.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized data.
        /// </returns>
        public string Serialize(T value) => base.Serialize(value);

        /// <summary>
        /// Data deserializer implementation proxy.
        /// </summary>
        /// <param name="data">
        /// Data to deserialized
        /// </param>
        /// <param name="defaultValue">
        /// The default value returned if deserialize throws an exception.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        public T Deserialize(string data, T defaultValue = default) => (T)base.Deserialize(data, defaultValue);
    
        internal void Set(Func<T, string> serialize, Func<string, T, T> deserialize)
        {
            Set(new Func<object, string>(obj => serialize((T)obj)),
                new Func<string, object, object>((data, defaultValue) => deserialize(data, (T)defaultValue)));
        }
    }
}
