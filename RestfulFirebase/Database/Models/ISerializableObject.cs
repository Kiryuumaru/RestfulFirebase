using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides convenient model for object serialization and encryption
    /// </summary>
    public interface ISerializableObject
    {
        /// <summary>
        /// Load the serialized value to the object.
        /// </summary>
        /// <param name="serialized">
        /// The serialized value to load.
        /// </param>
        void LoadFromSerializedValue(string serialized);

        /// <summary>
        /// Load the serialized value to the object.
        /// </summary>
        /// <param name="serialized">
        /// The serialized value to load.
        /// </param>
        /// <param name="encryptionPattern">
        /// The pattern for encryption used by the serialized value
        /// </param>
        void LoadFromSerializedValue(string serialized, int[] encryptionPattern);

        /// <summary>
        /// Generates the serialized value of the object.
        /// </summary>
        /// <returns>
        /// The serialized value of the object
        /// </returns>
        string GenerateSerializedValue();

        /// <summary>
        /// Generates the serialized value of the object.
        /// </summary>
        /// <param name="encryptionPattern">
        /// The pattern for encryption used by the serialized value
        /// </param>
        /// <returns>
        /// The serialized value of the object
        /// </returns>
        string GenerateSerializedValue(int[] encryptionPattern);
    }
}
