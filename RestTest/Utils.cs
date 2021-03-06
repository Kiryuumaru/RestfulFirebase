using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestTest
{
    internal static class Utils
    {
        #region Properties

        private const string NullIdentifier = "-";
        private const string EmptyIdentifier = "_";

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        internal const string Base64Charset = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        internal const string Base62Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        internal const string Base38Charset = "-0123456789_abcdefghijklmnopqrstuvwxyz";
        internal const string Base36Charset = "0123456789abcdefghijklmnopqrstuvwxyz";
        internal const string Base32Charset = "2345678abcdefghijklmnpqrstuvwxyz"; // Excluded 0, 1, 9, o

        #endregion

        #region Serializer

        internal static string[] Split(string data, params int[] lengths)
        {
            int sizes = 0;
            foreach (int size in lengths) sizes += size;
            if (sizes != data.Length) throw new Exception("Split error");

            string[] datas = new string[lengths.Length];
            int lastIndex = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = data.Substring(lastIndex, lengths[i]);
                lastIndex += lengths[i];
            }
            return datas;
        }

        internal static string BlobGetValue(string[] blobArray, string key, string defaultValue = "")
        {
            if (blobArray == null) return defaultValue;
            else if (blobArray.Length <= 1) return defaultValue;
            else if (blobArray.Length % 2 != 0) return defaultValue;
            int keyIndex = blobArray.ToList().IndexOf(key);
            if (keyIndex != 1 && keyIndex % 2 == 0 && (keyIndex + 1) < blobArray.Length) return blobArray[keyIndex + 1];
            else return defaultValue;
        }

        internal static string BlobGetValue(string blob, string key, string defaultValue = "")
        {
            var blobArray = DeserializeString(blob);
            return BlobGetValue(blobArray, key, defaultValue);
        }

        internal static string[] BlobSetValue(string[] blobArray, string key, string value)
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

        internal static string BlobSetValue(string blob, string key, string value)
        {
            var blobArray = DeserializeString(blob);
            return SerializeString(BlobSetValue(blobArray, key, value));
        }

        internal static string[] BlobDeleteValue(string[] blobArray, string key)
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

        internal static string BlobDeleteValue(string blob, string key)
        {
            var blobArray = DeserializeString(blob);
            return SerializeString(BlobDeleteValue(blobArray, key));
        }

        internal static string BlobConvert(Dictionary<string, string> dictionary)
        {
            string blob = "";
            foreach (var pair in dictionary)
            {
                blob = BlobSetValue(blob, pair.Key, pair.Value);
            }
            return blob;
        }

        internal static Dictionary<string, string> BlobConvert(string blob)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            var blobArray = DeserializeString(blob);
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

        internal static string SerializeString(params string[] datas)
        {
            if (datas == null) return NullIdentifier;
            if (datas.Length == 0) return EmptyIdentifier;
            var dataLength = ToBase62(datas.Length);
            var lengths = datas.Select(i => i == null ? NullIdentifier : (i == "" ? EmptyIdentifier : ToBase62(i.Length))).ToArray();
            int maxDigitLength = Math.Max(lengths.Max(i => i.Length), dataLength.Length);
            var maxDigitLength62 = ToBase62(maxDigitLength); ;
            for (int i = 0; i < datas.Length; i++)
            {
                lengths[i] = lengths[i].PadLeft(maxDigitLength, Base62Charset[0]);
            }
            var lengthsAndDatas = new string[lengths.Length + datas.Length];
            Array.Copy(lengths, lengthsAndDatas, lengths.Length);
            Array.Copy(datas, 0, lengthsAndDatas, lengths.Length, datas.Length);
            var joinedLengthsAndDatas = string.Join("", lengthsAndDatas);
            string serialized = string.Join("", maxDigitLength62, dataLength.PadLeft(maxDigitLength, Base62Charset[0]));
            var joinedArr = new string[] { serialized, joinedLengthsAndDatas };
            return string.Join("", joinedArr);
        }

        internal static string[] DeserializeString(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            if (data.Equals(NullIdentifier)) return null;
            if (data.Equals(EmptyIdentifier)) return Array.Empty<string>();
            if (data.Length < 4) return new string[] { "" };
            var d = (string)data.Clone();

            int indexDigits = FromBase62(d[0].ToString());
            int indexCount = FromBase62(d.Substring(1, indexDigits));
            var indices = d.Substring(1 + indexDigits, indexDigits * indexCount);
            var dataPart = d.Substring(1 + indexDigits + (indexDigits * indexCount));
            string[] datas = new string[indexCount];
            var currIndex = 0;
            for (int i = 0; i < indexCount; i++)
            {
                var subData = indices.Substring(indexDigits * i, indexDigits).TrimStart(Base62Charset[0]);
                if (subData.Equals(NullIdentifier)) datas[i] = null;
                else if (subData.Equals(EmptyIdentifier)) datas[i] = "";
                else
                {
                    var currLength = FromBase62(subData);
                    datas[i] = dataPart.Substring(currIndex, currLength);
                    currIndex += currLength;
                }
            }
            return datas;
        }

        #endregion

        #region StringArraySerializer

        internal static string ToBase62(int number)
        {
            var arbitraryBase = ToUnsignedArbitraryBaseSystem((ulong)number, 62);
            string base62 = "";
            foreach (var num in arbitraryBase)
            {
                base62 += Base62Charset[(int)num];
            }
            return base62;
        }

        internal static int FromBase62(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base62Charset.IndexOf(num);
                if (indexOf == -1) throw new Exception("Unknown charset");
                indexes.Add((uint)indexOf);
            }
            return (int)ToUnsignedNormalBaseSystem(indexes.ToArray(), 62);
        }

        internal static string ToBase64(int number)
        {
            var arbitraryBase = ToUnsignedArbitraryBaseSystem((ulong)number, 64);
            string base64 = "";
            foreach (var num in arbitraryBase)
            {
                base64 += Base64Charset[(int)num];
            }
            return base64;
        }

        internal static int FromBase64(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base64Charset.IndexOf(num);
                if (indexOf == -1) throw new Exception("Unknown charset");
                indexes.Add((uint)indexOf);
            }
            return (int)ToUnsignedNormalBaseSystem(indexes.ToArray(), 64);
        }

        #endregion

        #region EncryptString

        internal static string EncryptString(string value, int[] pattern)
        {
            if (value == null)
            {
                return null;
            }

            int patternIndex = 0;
            string result = "";
            for (int i = 0; i < value.Length; i++)
            {
                if (char.MaxValue < (char)(value[i] + pattern[patternIndex]))
                {
                    result += char.MaxValue - (char)(value[i] + pattern[patternIndex]);
                }
                else
                {
                    result += (char)(value[i] + pattern[patternIndex]);
                }
                patternIndex = (patternIndex + 1) >= pattern.Length ? 0 : patternIndex + 1;
            }
            return result;
        }

        internal static string DecryptString(string encrypted, int[] pattern)
        {
            if (encrypted == null)
            {
                return null;
            }

            int patternIndex = 0;
            string result = "";
            for (int i = 0; i < encrypted.Length; i++)
            {
                if (char.MinValue > (char)(encrypted[i] - pattern[patternIndex]))
                {
                    result += char.MaxValue - (char)(encrypted[i] - pattern[patternIndex]);
                }
                else
                {
                    result += (char)(encrypted[i] - pattern[patternIndex]);
                }
                patternIndex = (patternIndex + 1) >= pattern.Length ? 0 : patternIndex + 1;
            }
            return result;
        }

        #endregion

        #region Math

        internal static uint[] ToUnsignedArbitraryBaseSystem(ulong number, uint baseSystem)
        {
            if (baseSystem < 2) throw new Exception("Base below 1 error");
            var baseArr = new List<uint>();
            while (number >= baseSystem)
            {
                var ans = number / baseSystem;
                var remainder = number % baseSystem;
                number = ans;
                baseArr.Add((uint)remainder);
            }
            baseArr.Add((uint)number);
            baseArr.Reverse();
            return baseArr.ToArray();
        }

        internal static ulong ToUnsignedNormalBaseSystem(uint[] arbitraryBaseNumber, uint baseSystem)
        {
            if (baseSystem < 2) throw new Exception("Base below 1 error");
            if (arbitraryBaseNumber.Any(i => i >= baseSystem)) throw new Exception("Number has greater value than base number system");
            ulong value = 0;
            var reverse = arbitraryBaseNumber.Reverse().ToArray();
            for (int i = 0; i < arbitraryBaseNumber.Length; i++)
            {
                value += (ulong)(reverse[i] * Math.Pow(baseSystem, i));
            }
            return value;
        }

        internal static uint[] ToSignedArbitraryBaseSystem(long number, uint baseSystem)
        {
            var num = ToUnsignedArbitraryBaseSystem((ulong)Math.Abs(number), baseSystem);
            var newNum = new uint[num.Length + 1];
            Array.Copy(num, 0, newNum, 1, num.Length);
            newNum[0] = number < 0 ? baseSystem - 1 : 0;
            return newNum;
        }

        internal static long ToSignedNormalBaseSystem(uint[] arbitraryBaseNumber, uint baseSystem)
        {
            bool isNegative;
            if (arbitraryBaseNumber[0] == 0) isNegative = false;
            else if (arbitraryBaseNumber[0] == baseSystem - 1) isNegative = true;
            else throw new Exception("Not a signed number");
            var num = (long)ToUnsignedNormalBaseSystem(arbitraryBaseNumber.Skip(1).ToArray(), baseSystem);
            return isNegative ? -num : num;
        }

        internal static double CalcVariance(IEnumerable<double> datas)
        {
            double mean = datas.Average();
            double sum = 0;
            foreach (double data in datas) sum += Math.Pow(data - mean, 2);
            return sum / (datas.Count() - 1);
        }

        internal static double CalcStandardDeviation(IEnumerable<double> datas)
        {
            double mean = datas.Average();
            double sum = 0;
            foreach (double data in datas) sum += Math.Pow(data - mean, 2);
            return Math.Pow(sum / datas.Count(), 0.5);
        }

        #endregion

        #region TaskUtils

        /// <summary>
		/// Task extension to add a timeout.
		/// </summary>
		/// <returns>The task with timeout.</returns>
		/// <param name="task">Task.</param>
		/// <param name="timeoutInMilliseconds">Timeout duration in Milliseconds.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		internal async static Task<T> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds)
        {
            var retTask = await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)).ConfigureAwait(false);
            return retTask is Task<T> ? task.Result : default;
        }

        /// <summary>
        /// Task extension to add a timeout.
        /// </summary>
        /// <returns>The task with timeout.</returns>
        /// <param name="task">Task.</param>
        /// <param name="timeout">Timeout Duration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        internal static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout) => WithTimeout(task, (int)timeout.TotalMilliseconds);

        /// <summary>
        /// Attempts to await on the task and catches exception
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="onException">What to do when method has an exception</param>
        /// <param name="continueOnCapturedContext">If the context should be captured.</param>
        internal static async void SafeFireAndForget(this Task task, Action<Exception> onException = null, bool continueOnCapturedContext = false)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException.Invoke(ex);
            }
        }

        #endregion

        #region UrlUtils

        internal static string CombineUrl(params string[] paths)
        {
            string ret = "";
            foreach (var path in paths)
            {
                if (path.EndsWith("/")) ret += path;
                else ret += (path + "/");
            }
            return ret.Length == 0 ? "/" : ret;
        }

        internal static string[] SeparateUrl(string url)
        {
            var split = url.Split('/');
            if (!string.IsNullOrEmpty(split.Last())) return split;
            return split.Take(split.Length - 1).ToArray();
        }

        #endregion
    }
}
