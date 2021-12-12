using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Serializers
{
    /// <summary>
    /// Provides declaration for type serialization methods.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to serialize and deserialize.
    /// </typeparam>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serializes <typeparamref name="T"/> to its corresponding <see cref="string"/>.
        /// </summary>
        /// <param name="value">
        /// The value to serialize.
        /// </param>
        /// <returns>
        /// The serialized value.
        /// </returns>
        string Serialize(T value);

        /// <summary>
        /// Deserialize <see cref="string"/> to its corresponding <typeparamref name="T"/>.
        /// </summary>
        /// <param name="data">
        /// The serialized data to deserialize.
        /// </param>
        /// <param name="defaultValue">
        /// The default value if the serialization fail.
        /// </param>
        /// <returns>
        /// The deserialized value.
        /// </returns>
        T Deserialize(string data, T defaultValue = default);
    }
}
