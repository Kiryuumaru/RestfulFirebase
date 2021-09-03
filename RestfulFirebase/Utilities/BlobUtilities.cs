using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Provides a serializable key-value pair utilities.
    /// </summary>
    public static class BlobUtilities
    {
        /// <summary>
        /// Gets value from the provided <paramref name="blobArray"/> parameter.
        /// </summary>
        /// <param name="blobArray">
        /// The blob to get the value from.
        /// </param>
        /// <param name="key">
        /// The key of the value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value if the value was not found.
        /// </param>
        /// <returns>
        /// The value of the provided <paramref name="key"/> from the provided <paramref name="blobArray"/> parameters.
        /// </returns>
        public static string GetValue(string[] blobArray, string key, string defaultValue = "")
        {
            if (blobArray == null) return defaultValue;
            else if (blobArray.Length <= 1) return defaultValue;
            else if (blobArray.Length % 2 != 0) return defaultValue;
            int keyIndex = blobArray.ToList().IndexOf(key);
            if (keyIndex != 1 && keyIndex % 2 == 0 && (keyIndex + 1) < blobArray.Length) return blobArray[keyIndex + 1];
            else return defaultValue;
        }

        /// <summary>
        /// Gets value from the provided <paramref name="blob"/> parameter.
        /// </summary>
        /// <param name="blob">
        /// The blob to get the value from.
        /// </param>
        /// <param name="key">
        /// The key of the value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value if the value was not found.
        /// </param>
        /// <returns>
        /// The value of the provided <paramref name="key"/> from the provided <paramref name="blob"/> parameters.
        /// </returns>
        public static string GetValue(string blob, string key, string defaultValue = "")
        {
            var blobArray = StringUtilities.Deserialize(blob);
            return GetValue(blobArray, key, defaultValue);
        }

        /// <summary>
        /// Sets key-value pair to the provided <paramref name="blobArray"/> parameter.
        /// </summary>
        /// <param name="blobArray">
        /// The blob to set to.
        /// </param>
        /// <param name="key">
        /// The key of the value to set.
        /// </param>
        /// <param name="value">
        /// The value to set.
        /// </param>
        /// <returns>
        /// The resulting blob.
        /// </returns>
        public static string[] SetValue(string[] blobArray, string key, string value)
        {
            if (blobArray == null) blobArray = Array.Empty<string>();
            else if (blobArray.Length <= 1) blobArray = Array.Empty<string>();
            else if (blobArray.Length % 2 != 0) blobArray = Array.Empty<string>();
            int keyIndex = blobArray.ToList().IndexOf(key);
            if (keyIndex != 1 && keyIndex % 2 == 0 && (keyIndex + 1) < blobArray.Length)
            {
                blobArray[keyIndex + 1] = value;
                return blobArray;
            }
            else
            {
                var newBlobArray = blobArray.ToList();
                newBlobArray.Add(key);
                newBlobArray.Add(value);
                return newBlobArray.ToArray();
            }
        }

        /// <summary>
        /// Sets key-value pair to the provided <paramref name="blob"/> parameter.
        /// </summary>
        /// <param name="blob">
        /// The blob to set to.
        /// </param>
        /// <param name="key">
        /// The key of the value to set.
        /// </param>
        /// <param name="value">
        /// The value to set.
        /// </param>
        /// <returns>
        /// The resulting blob.
        /// </returns>
        public static string SetValue(string blob, string key, string value)
        {
            var blobArray = StringUtilities.Deserialize(blob);
            return StringUtilities.Serialize(SetValue(blobArray, key, value));
        }

        /// <summary>
        /// Deletes value from the provided <paramref name="blobArray"/> parameter.
        /// </summary>
        /// <param name="blobArray">
        /// The blob to delete from.
        /// </param>
        /// <param name="key">
        /// The key of the value to delete.
        /// </param>
        /// <returns>
        /// The resulting blob.
        /// </returns>
        public static string[] DeleteValue(string[] blobArray, string key)
        {
            if (blobArray == null) return blobArray;
            else if (blobArray.Length <= 1) return blobArray;
            else if (blobArray.Length % 2 != 0) return blobArray;
            int keyIndex = blobArray.ToList().IndexOf(key);
            if (keyIndex != 1 && keyIndex % 2 == 0 && (keyIndex + 1) < blobArray.Length)
            {
                var newBlobArray = blobArray.ToList();
                newBlobArray.RemoveAt(keyIndex);
                newBlobArray.RemoveAt(keyIndex);
                return newBlobArray.ToArray();
            }
            else return blobArray;
        }

        /// <summary>
        /// Deletes value from the provided <paramref name="blob"/> parameter.
        /// </summary>
        /// <param name="blob">
        /// The blob to delete from.
        /// </param>
        /// <param name="key">
        /// The key of the value to delete.
        /// </param>
        /// <returns>
        /// The resulting blob.
        /// </returns>
        public static string DeleteValue(string blob, string key)
        {
            var blobArray = StringUtilities.Deserialize(blob);
            return StringUtilities.Serialize(DeleteValue(blobArray, key));
        }

        /// <summary>
        /// Converts dictionary to blob.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary to convert.
        /// </param>
        /// <returns>
        /// The converted blob from the provided <paramref name="dictionary"/> parameter.
        /// </returns>
        public static string Convert(Dictionary<string, string> dictionary)
        {
            string blob = "";
            foreach (var pair in dictionary)
            {
                blob = SetValue(blob, pair.Key, pair.Value);
            }
            return blob;
        }

        /// <summary>
        /// Converts blob to dictionary.
        /// </summary>
        /// <param name="blob">
        /// The blob to convert.
        /// </param>
        /// <returns>
        /// The converted dictionary from the provided <paramref name="blob"/> parameter.
        /// </returns>
        public static Dictionary<string, string> Convert(string blob)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            var blobArray = StringUtilities.Deserialize(blob);
            if (blobArray == null) return dictionary;
            else if (blobArray.Length <= 1) return dictionary;
            else if (blobArray.Length % 2 != 0) return dictionary;
            List<string> keys = new List<string>();
            List<string> values = new List<string>();
            for (int i = 0; i < blobArray.Length; i++)
            {
                if (i != 1 && i % 2 == 0 && (i + 1) < blobArray.Length) keys.Add(blobArray[i]);
                else values.Add(blobArray[i]);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                dictionary.Add(keys[i], values[i]);
            }
            return dictionary;
        }
    }
}
