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
        /// <param name="removeNulls">
        /// Specify whether to remove <see cref="JTokenType.Null"/> from the array.
        /// </param>
        public static void ConvertAllArrayToObject(this JToken token, bool removeNulls = true)
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
                    if (!removeNulls || arr[i].Type != JTokenType.Null)
                    {
                        arrObj[i.ToString()] = arr[i];
                    }
                }
                token.Replace(arrObj);
            }
        }
    }
}
