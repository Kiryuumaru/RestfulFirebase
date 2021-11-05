using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Provides extention methods for <see cref="JToken"/> class.
    /// </summary>
    public static class JTokenExtensions
    {
        /// <summary>
        /// Converts all <see cref="JArray"/> to <see cref="JObject"/> using its index as its property key recursively.
        /// </summary>
        /// <param name="token">
        /// The <see cref="JToken"/> to convert.
        /// </param>
        /// <param name="removeArrayNulls">
        /// Specify whether to remove <see cref="JTokenType.Null"/> from the array.
        /// </param>
        public static void ConvertAllArrayToObject(this JToken token, bool removeArrayNulls = true)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (var subToken in token as JObject)
                {
                    ConvertAllArrayToObject(subToken.Value);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                JObject arrObj = new JObject();
                JArray arr = token as JArray;
                for (int i = 0; i < arr.Count; i++)
                {
                    ConvertAllArrayToObject(arr[i]);
                    if (!removeArrayNulls || arr[i].Type != JTokenType.Null)
                    {
                        arrObj[i.ToString()] = arr[i];
                    }
                }
                token.Replace(arrObj);
            }
        }

        /// <summary>
        /// Gets the hierarchy in flat list with path separated with '/'.
        /// </summary>
        /// <param name="token">
        /// The <see cref="JToken"/> to flat the hierarchy.
        /// </param>
        /// <param name="removeArrayNulls">
        /// Specify whether to remove <see cref="JTokenType.Null"/> from the array.
        /// </param>
        /// <returns>
        /// The list of flat hierarchy tuple path with value.
        /// </returns>
        public static IDictionary<string[], object> GetFlatHierarchy(this JToken token, bool removeArrayNulls = true)
        {
            Dictionary<string[], object> descendants = new Dictionary<string[], object>();

            void recursive(JToken recToken, string[] path)
            {
                if (recToken is JObject jRecObject)
                {
                    foreach (var subToken in jRecObject)
                    {
                        string[] subPath = new string[path.Length + 1];
                        Array.Copy(path, 0, subPath, 0, path.Length);
                        subPath[subPath.Length - 1] = subToken.Key;
                        recursive(subToken.Value, subPath);
                    }
                }
                else if (recToken is JArray jRecArray)
                {
                    for (int i = 0; i < jRecArray.Count; i++)
                    {
                        if (!removeArrayNulls || jRecArray[i].Type != JTokenType.Null)
                        {
                            string[] subPath = new string[path.Length + 1];
                            Array.Copy(path, 0, subPath, 0, path.Length);
                            subPath[subPath.Length - 1] = i.ToString();
                            recursive(jRecArray[i], subPath);
                        }
                    }
                }
                else if (recToken is JValue jRecValue)
                {
                    descendants.Add(path, jRecValue.Value);
                }
                else
                {
                    descendants.Add(path, null);
                }
            }

            if (token is JObject jObject)
            {
                foreach (var subToken in jObject)
                {
                    string[] subPath = new string[] { subToken.Key };
                    recursive(subToken.Value, subPath);
                }
            }
            else if (token is JArray jArray)
            {
                for (int i = 0; i < jArray.Count; i++)
                {
                    if (!removeArrayNulls || jArray[i].Type != JTokenType.Null)
                    {
                        string[] subPath = new string[] { i.ToString() };
                        recursive(jArray[i], subPath);
                    }
                }
            }
            else if (token is JValue jValue)
            {
                descendants.Add(new string[0], jValue.Value);
            }
            else
            {
                descendants.Add(new string[0], null);
            }

            return descendants;
        }
    }
}
