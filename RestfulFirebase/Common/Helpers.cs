using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Common
{
    public static class Helpers
    {
        #region UIDGenerator

        // Modeled after base64 web-safe chars, but ordered by ASCII.
        private const string Base64Charset = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        private const string Base62Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string AlphanumericNonCaseSensitive = "0123456789abcdefghijklmnopqrstuvwxyz";
        private static readonly char[] PushChars = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(Base64Charset));
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private static readonly Random random = new Random();
        private static readonly byte[] lastRandChars = new byte[12];

        private static long lastPushTime;

        public static string GenerateUID(int length = 10, bool isCaseSensetive = true)
        {
            string id = "";
            for (int i = 0; i < length; i++)
            {
                id += isCaseSensetive ?
                    Base62Charset[random.Next(Base62Charset.Length)] :
                    AlphanumericNonCaseSensitive[random.Next(AlphanumericNonCaseSensitive.Length)];
            }
            return id;
        }

        public static string GenerateSafeUID()
        {
            var id = new StringBuilder(20);
            var now = (long)(DateTimeOffset.Now - Epoch).TotalMilliseconds;
            var duplicateTime = now == lastPushTime;
            lastPushTime = now;

            var timeStampChars = new char[8];
            for (int i = 7; i >= 0; i--)
            {
                var index = (int)(now % PushChars.Length);
                timeStampChars[i] = PushChars[index];
                now = (long)Math.Floor((double)now / PushChars.Length);
            }

            if (now != 0)
            {
                throw new Exception("We should have converted the entire timestamp.");
            }

            id.Append(timeStampChars);

            if (!duplicateTime)
            {
                for (int i = 0; i < 12; i++)
                {
                    lastRandChars[i] = (byte)random.Next(0, PushChars.Length);
                }
            }
            else
            {
                var lastIndex = 11;
                for (; lastIndex >= 0 && lastRandChars[lastIndex] == PushChars.Length - 1; lastIndex--)
                {
                    lastRandChars[lastIndex] = 0;
                }

                lastRandChars[lastIndex]++;
            }

            for (int i = 0; i < 12; i++)
            {
                id.Append(PushChars[lastRandChars[i]]);
            }

            if (id.Length != 20)
            {
                throw new Exception("Length should be 20.");
            }

            return id.ToString();
        }

        #endregion

        #region Serializer

        public static string[] Split(string data, params int[] lengths)
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

        public static string EncodeDateTime(DateTime dateTime)
        {
            string data = "";
            data += dateTime.Year.ToString("0000");
            data += dateTime.Month.ToString("00");
            data += dateTime.Day.ToString("00");
            data += dateTime.Hour.ToString("00");
            data += dateTime.Minute.ToString("00");
            data += dateTime.Second.ToString("00");
            data += dateTime.Millisecond.ToString("000");
            return data;
        }

        public static DateTime DecodeDateTime(string data, DateTime defaultValue)
        {
            var decoded = DecodeDateTime(data);
            return decoded.HasValue ? decoded.Value : defaultValue;
        }

        public static DateTime? DecodeDateTime(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            try
            {
                string[] datas = Split(data, 4, 2, 2, 2, 2, 2, 3);
                int year = Convert.ToInt32(datas[0]);
                int month = Convert.ToInt32(datas[1]);
                int day = Convert.ToInt32(datas[2]);
                int hour = Convert.ToInt32(datas[3]);
                int minute = Convert.ToInt32(datas[4]);
                int second = Convert.ToInt32(datas[5]);
                int millisecond = Convert.ToInt32(datas[6]);
                return new DateTime(year, month, day, hour, minute, second, millisecond);
            }
            catch { return null; }
        }

        //public static string EncodeDateTime(DateTime date)
        //{
        //    long shortTicks = (date.Ticks - 631139040000000000L) / 10000L;
        //    var bytes = BitConverter.GetBytes(shortTicks);
        //    if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
        //    return Convert.ToBase64String(bytes).Substring(0, 7);
        //}

        //public static DateTime DecodeDateTime(string encodedTimestamp, DateTime defaultValue)
        //{
        //    var dateTime = DecodeDateTime(encodedTimestamp);
        //    return dateTime.HasValue ? dateTime.Value : defaultValue;
        //}

        //public static DateTime? DecodeDateTime(string encodedTimestamp)
        //{
        //    if (string.IsNullOrEmpty(encodedTimestamp)) return null;
        //    try
        //    {
        //        byte[] data = new byte[8];
        //        Convert.FromBase64String(encodedTimestamp + "AAAA=").CopyTo(data, 0);
        //        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        //        return new DateTime((BitConverter.ToInt64(data, 0) * 10000L) + 631139040000000000L);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        public static string BlobGetValue(string blob, string key, string defaultValue = "")
        {
            var blobArray = DeserializeString(blob);
            if (blobArray == null) return defaultValue;
            else if (blobArray.Length <= 1) return defaultValue;
            else if (blobArray.Length % 2 != 0) return defaultValue;
            int keyIndex = blobArray.ToList().IndexOf(key);
            if (keyIndex != 1 && keyIndex % 2 == 0 && (keyIndex + 1) < blobArray.Length) return blobArray[keyIndex + 1];
            else return defaultValue;
        }

        public static string BlobSetValue(string blob, string key, string value)
        {
            var blobArray = DeserializeString(blob);
            if (blobArray == null) blobArray = Array.Empty<string>();
            else if (blobArray.Length <= 1) blobArray = Array.Empty<string>();
            else if (blobArray.Length % 2 != 0) blobArray = Array.Empty<string>();
            int keyIndex = blobArray.ToList().IndexOf(key);
            if (keyIndex != 1 && keyIndex % 2 == 0 && (keyIndex + 1) < blobArray.Length)
            {
                blobArray[keyIndex + 1] = value;
                return SerializeString(blobArray);
            }
            else
            {
                var newBlobArray = blobArray.ToList();
                newBlobArray.Add(key);
                newBlobArray.Add(value);
                return SerializeString(newBlobArray.ToArray());
            }
        }

        public static string BlobDeleteValue(string blob, string key)
        {
            var blobArray = DeserializeString(blob);
            if (blobArray == null) return blob;
            else if (blobArray.Length <= 1) return blob;
            else if (blobArray.Length % 2 != 0) return blob;
            int keyIndex = blobArray.ToList().IndexOf(key);
            if (keyIndex != 1 && keyIndex % 2 == 0 && (keyIndex + 1) < blobArray.Length)
            {
                var newBlobArray = blobArray.ToList();
                newBlobArray.RemoveAt(keyIndex);
                newBlobArray.RemoveAt(keyIndex);
                return SerializeString(newBlobArray.ToArray());
            }
            else return blob;
        }


        public static string BlobConvert(Dictionary<string, string> dictionary)
        {
            string blob = "";
            foreach (var pair in dictionary)
            {
                blob = BlobSetValue(blob, pair.Key, pair.Value);
            }
            return blob;
        }

        public static Dictionary<string, string> BlobConvert(string blob)
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

        public static string JsonConvert(Dictionary<string, string> dictionary)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(dictionary);
        }

        public static Dictionary<string, string> JsonConvert(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        #endregion

        #region UIStringer

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public static string GetFormattedTimeSpan(TimeSpan timeSpan)
        {
            double delta = Math.Abs(timeSpan.TotalSeconds);

            if (delta < 1 * Minute)
                return "just now";

            if (delta < 2 * Minute)
                return "a minute";

            if (delta < 45 * Minute)
                return timeSpan.Minutes + " minutes";

            if (delta < 90 * Minute)
                return "an hour";

            if (delta < 24 * Hour)
                return timeSpan.Hours + " hours";

            if (delta < 48 * Hour)
                return "yesterday";

            if (delta < 30 * Day)
                return timeSpan.Days + " days";

            if (delta < 12 * Month)
            {
                int months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
                return months <= 1 ? "a month" : months + " months";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
                return years <= 1 ? "a year" : years + " years";
            }
        }

        #endregion

        #region ByteSerializer

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        CopyTo(msi, gs);
                    }
                    return mso.ToArray();
                }
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        CopyTo(gs, mso);
                    }
                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
        }

        #endregion

        #region StringArraySerializer

        private const string NullIdentifier = "-";
        private const string EmptyIdentifier = "_";

        private static string ToBase62(int number)
        {
            var arbitraryBase = ToArbitraryBaseSystem((ulong)number, 62);
            string base62 = "";
            foreach (var num in arbitraryBase)
            {
                base62 += Base62Charset[(int)num];
            }
            return base62;
        }

        private static int FromBase62(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base62Charset.IndexOf(num);
                if (indexOf == -1) throw new Exception("Unknown charset");
                indexes.Add((uint)indexOf);
            }
            return (int)ToNormalBaseSystem(indexes.ToArray(), 62);
        }

        private static string ToBase64(int number)
        {
            var arbitraryBase = ToArbitraryBaseSystem((ulong)number, 64);
            string base64 = "";
            foreach (var num in arbitraryBase)
            {
                base64 += Base64Charset[(int)num];
            }
            return base64;
        }

        private static int FromBase64(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base64Charset.IndexOf(num);
                if (indexOf == -1) throw new Exception("Unknown charset");
                indexes.Add((uint)indexOf);
            }
            return (int)ToNormalBaseSystem(indexes.ToArray(), 64);
        }

        public static string SerializeString2(params string[] datas)
        {
            if (datas == null) return NullIdentifier;
            if (datas.Length == 0) return EmptyIdentifier;
            var dataLength = ToBase62(datas.Length);
            var lengths = datas.Select(i => i == null ? NullIdentifier : (i == "" ? EmptyIdentifier : ToBase62(i.Length))).ToArray();
            int maxDigitLength = Math.Max(lengths.Max(i => i == null ? 0 : i.Length), dataLength.Length);
            var maxDigitLength62 = ToBase62(maxDigitLength);
            string serialized = maxDigitLength62 + dataLength.PadLeft(maxDigitLength, Base62Charset[0]);
            for (int i = 0; i < datas.Length; i++)
            {
                serialized += lengths[i].PadLeft(maxDigitLength, Base62Charset[0]);
            }
            for (int i = 0; i < datas.Length; i++)
            {
                serialized += datas[i];
            }
            return serialized;
        }

        public static string[] DeserializeString2(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            if (data.Equals(NullIdentifier)) return null;
            if (data.Equals(EmptyIdentifier)) return Array.Empty<string>();
            if (data.Length < 4) return new string[] { "" };
            var d = (string)data.Clone();
            try
            {
                int indexDigits = FromBase62(d[0].ToString());
                d = d.Substring(1);
                int indexCount = FromBase62(d.Substring(0, indexDigits));
                d = d.Substring(indexDigits);
                int[] lengths = new int[indexCount];
                for (int i = 0; i < lengths.Length; i++)
                {
                    var subData = d.Substring(0, indexDigits).TrimStart(Base62Charset[0]);
                    d = d.Substring(indexDigits);
                    if (subData.Equals(NullIdentifier)) lengths[i] = -1;
                    else if (subData.Equals(EmptyIdentifier)) lengths[i] = 0;
                    else lengths[i] = FromBase62(subData);
                }
                string[] datas = new string[indexCount];
                for (int i = 0; i < datas.Length; i++)
                {
                    if (lengths[i] == -1) datas[i] = null;
                    else if (lengths[i] == 0) datas[i] = "";
                    else
                    {
                        datas[i] = d.Substring(0, lengths[i]);
                        d = d.Substring(lengths[i]);
                    }
                }
                return datas;
            }
            catch { return null; }
        }

        public static string SerializeString(params string[] datas)
        {
            if (datas == null) return NullIdentifier;
            if (datas.Length == 0) return EmptyIdentifier;
            int maxLength = datas.Max(i => i == null ? 0 : i.Length);
            int indexDigits = Math.Max(datas.Length.ToString().Length, Math.Max(maxLength.ToString().Length, Math.Max(NullIdentifier.Length, EmptyIdentifier.Length)));
            string serializedDataHeader = datas.Length.ToString("D" + indexDigits);
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] == null) serializedDataHeader += (indexDigits - NullIdentifier.Length == 0 ? "" : 0.ToString("D0" + (indexDigits - NullIdentifier.Length))) + NullIdentifier;
                else serializedDataHeader += datas[i].Length.ToString("D" + indexDigits);
            }
            string dataBody = "";
            for (int i = 0; i < datas.Length; i++)
            {
                dataBody += datas[i];
            }
            return indexDigits.ToString() + serializedDataHeader + dataBody;
        }

        public static string[] DeserializeString(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            if (data.Equals(NullIdentifier)) return null;
            if (data.Equals(EmptyIdentifier)) return Array.Empty<string>();
            if (data.Length < 4) return new string[] { "" };
            try
            {
                int indexDigits = int.Parse(data[0].ToString());
                data = data.Substring(1);
                int indexCount = int.Parse(data.Substring(0, indexDigits));
                data = data.Substring(indexDigits);
                int[] lengths = new int[indexCount];
                for (int i = 0; i < lengths.Length; i++)
                {
                    var subData = data.Substring(0, indexDigits);
                    data = data.Substring(indexDigits);
                    if (subData.Equals((indexDigits - NullIdentifier.Length == 0 ? "" : 0.ToString("D0" + (indexDigits - NullIdentifier.Length))) + NullIdentifier)) lengths[i] = -1;
                    else lengths[i] = int.Parse(subData);
                }
                string[] datas = new string[indexCount];
                for (int i = 0; i < datas.Length; i++)
                {
                    if (lengths[i] == -1) datas[i] = null;
                    else
                    {
                        datas[i] = data.Substring(0, lengths[i]);
                        data = data.Substring(lengths[i]);
                    }
                }
                return datas;
            }
            catch { return null; }
        }

        #endregion

        #region Math

        public static uint[] ToArbitraryBaseSystem(ulong number, uint baseSystem)
        {
            if (baseSystem < 2) throw new Exception("Base below 1 error");
            if (number < 0) throw new Exception("Number below zero error");
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

        public static ulong ToNormalBaseSystem(uint[] arbitraryBaseNumber, uint baseSystem)
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

        public static double CalcVariance(IEnumerable<double> datas)
        {
            double mean = datas.Average();
            double sum = 0;
            foreach (double data in datas) sum += Math.Pow(data - mean, 2);
            return sum / (datas.Count() - 1);
        }

        public static double CalcStandardDeviation(IEnumerable<double> datas)
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
		public async static Task<T> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds)
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
        public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout) => WithTimeout(task, (int)timeout.TotalMilliseconds);

        /// <summary>
        /// Attempts to await on the task and catches exception
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="onException">What to do when method has an exception</param>
        /// <param name="continueOnCapturedContext">If the context should be captured.</param>
        public static async void SafeFireAndForget(this Task task, Action<Exception> onException = null, bool continueOnCapturedContext = false)
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
    }
}
