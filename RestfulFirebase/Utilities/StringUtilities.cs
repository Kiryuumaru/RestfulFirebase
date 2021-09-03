using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Provides <see cref="string"/> extensions.
    /// </summary>
    public static class StringUtilities
    {
        private const string NullIdentifier = "-";
        private const string EmptyIdentifier = "_";

        /// <summary>
        /// A web safe Base64 charset. Alphanumeric characters with '-' and '_' characters.
        /// </summary>
        public const string Base64Charset = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// A web safe Base62 charset. Alphanumeric characters.
        /// </summary>
        public const string Base62Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// A web safe Base38 charset. Numbers and lower-case letters with '-' and '_' characters.
        /// </summary>
        public const string Base38Charset = "-0123456789_abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// A web safe Base36 charset. Numbers and lower-case letters.
        /// </summary>
        public const string Base36Charset = "0123456789abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// A web safe Base32 charset, excluded 0, 1, 9, o from Base36.
        /// </summary>
        public const string Base32Charset = "2345678abcdefghijklmnpqrstuvwxyz";

        /// <summary>
        /// Splits the provided string with its respective sub string lengths.
        /// </summary>
        /// <param name="value">
        /// The value to split.
        /// </param>
        /// <param name="lengths">
        /// The lengths of the sub string to split.
        /// </param>
        /// <returns>
        /// The splitted array of <see cref="string"/> from the provided <paramref name="value"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws when the total provided <paramref name="lengths"/> total is outside the length of the provided <paramref name="value"/>.
        /// </exception>
        public static string[] Split(this string value, params int[] lengths)
        {
            int sizes = 0;
            foreach (int size in lengths) sizes += size;
            if (sizes != value.Length) throw new ArgumentOutOfRangeException(nameof(lengths), "Provided lengths total is outside the length of the provided value.");

            string[] datas = new string[lengths.Length];
            int lastIndex = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = value.Substring(lastIndex, lengths[i]);
                lastIndex += lengths[i];
            }
            return datas;
        }

        /// <summary>
        /// Serializes an array of <see cref="string"/>.
        /// </summary>
        /// <param name="data">
        /// The array of string to serialize.
        /// </param>
        /// <returns>
        /// The serialized value of the provided <paramref name="data"/> parameter.
        /// </returns>
        public static string Serialize(params string[] data)
        {
            if (data == null) return NullIdentifier;
            if (data.Length == 0) return EmptyIdentifier;
            var dataLength = ToBase62(data.Length);
            var lengths = data.Select(i => i == null ? NullIdentifier : (string.IsNullOrEmpty(i) ? EmptyIdentifier : ToBase62(i.Length))).ToArray();
            int maxDigitLength = Math.Max(lengths.Max(i => i.Length), dataLength.Length);
            var maxDigitLength62 = ToBase62(maxDigitLength); ;
            for (int i = 0; i < data.Length; i++)
            {
                lengths[i] = lengths[i].PadLeft(maxDigitLength, Base62Charset[0]);
            }
            var lengthsAndDatas = new string[lengths.Length + data.Length];
            Array.Copy(lengths, lengthsAndDatas, lengths.Length);
            Array.Copy(data, 0, lengthsAndDatas, lengths.Length, data.Length);
            var joinedLengthsAndDatas = string.Join("", lengthsAndDatas);
            string serialized = string.Join("", maxDigitLength62, dataLength.PadLeft(maxDigitLength, Base62Charset[0]));
            var joinedArr = new string[] { serialized, joinedLengthsAndDatas };
            return string.Join("", joinedArr);
        }

        /// <summary>
        /// Deserializes data to array of <see cref="string"/>.
        /// </summary>
        /// <param name="data">
        /// The data from serializer to deserialize.
        /// </param>
        /// <returns>
        /// The deserialized array of <see cref="string"/> from the provided <paramref name="data"/> parameter.
        /// </returns>
        public static string[] Deserialize(string data)
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

        /// <summary>
        /// Converts number to its base62 value.
        /// </summary>
        /// <param name="number">
        /// The number to convert.
        /// </param>
        /// <returns>
        /// The base62 representation of the provided <paramref name="number"/> parameter.
        /// </returns>
        public static string ToBase62(int number)
        {
            var arbitraryBase = MathUtilities.ToUnsignedArbitraryBaseSystem((ulong)number, 62);
            StringBuilder builder = new StringBuilder();
            foreach (var num in arbitraryBase)
            {
                builder.Append(Base62Charset[(int)num]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Converts base62 representation number to its value.
        /// </summary>
        /// <param name="number">
        /// The number to convert.
        /// </param>
        /// <returns>
        /// The value of the provided base62 representation <paramref name="number"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws when the provided <paramref name="number"/> is not from a base62 value.
        /// </exception>
        public static int FromBase62(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base62Charset.IndexOf(num);
                if (indexOf == -1) throw new ArgumentOutOfRangeException("The number is not a base62 value.");
                indexes.Add((uint)indexOf);
            }
            return (int)MathUtilities.ToUnsignedNormalBaseSystem(indexes.ToArray(), 62);
        }

        /// <summary>
        /// Converts number to its base64 value.
        /// </summary>
        /// <param name="number">
        /// The number to convert.
        /// </param>
        /// <returns>
        /// The base64 representation of the provided <paramref name="number"/> parameter.
        /// </returns>
        public static string ToBase64(int number)
        {
            var arbitraryBase = MathUtilities.ToUnsignedArbitraryBaseSystem((ulong)number, 64);
            StringBuilder builder = new StringBuilder();
            foreach (var num in arbitraryBase)
            {
                builder.Append(Base64Charset[(int)num]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Converts base64 representation number to its value.
        /// </summary>
        /// <param name="number">
        /// The number to convert.
        /// </param>
        /// <returns>
        /// The value of the provided base64 representation <paramref name="number"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws when the provided <paramref name="number"/> is not from a base64 value.
        /// </exception>
        public static int FromBase64(string number)
        {
            var indexes = new List<uint>();
            foreach (var num in number)
            {
                var indexOf = Base64Charset.IndexOf(num);
                if (indexOf == -1) throw new ArgumentOutOfRangeException("The number is not a base64 value.");
                indexes.Add((uint)indexOf);
            }
            return (int)MathUtilities.ToUnsignedNormalBaseSystem(indexes.ToArray(), 64);
        }

        /// <summary>
        /// Converts <see cref="string"/> value to <see cref="byte"/> array.
        /// </summary>
        /// <param name="str">
        /// The value to convert.
        /// </param>
        /// <returns>
        /// The converted <see cref="byte"/> array from the provided <paramref name="str"/> parameter.
        /// </returns>
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

        /// <summary>
        /// Converts <see cref="byte"/> array to <see cref="string"/> value.
        /// </summary>
        /// <param name="bytes">
        /// The bytes to convert.
        /// </param>
        /// <returns>
        /// The converted <see cref="string"/> value from the provided <paramref name="bytes"/> parameter.
        /// </returns>
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

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}
