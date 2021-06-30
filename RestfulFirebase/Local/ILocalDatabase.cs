using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Local
{
    /// <summary>
    /// The local database declarations used for app persistency and offline database.
    /// </summary>
    public interface ILocalDatabase
    {
        /// <summary>
        /// Check if the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">
        /// The key to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="key"/> exists; otherwise <c>false</c>.
        /// </returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Gets the value of the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key to get.
        /// </param>
        /// <returns>
        /// The value of the specified <paramref name="key"/>.
        /// </returns>
        string Get(string key);

        /// <summary>
        /// Sets the <paramref name="value"/> of the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the value to set.
        /// </param>
        /// <param name="value">
        /// The value to set.
        /// </param>
        void Set(string key, string value);

        /// <summary>
        /// Deletes the value of the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the value to delete.
        /// </param>
        void Delete(string key);

        /// <summary>
        /// Clears the local database.
        /// </summary>
        void Clear();
    }
}
