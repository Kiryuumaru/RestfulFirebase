using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RestfulFirebase.Common.Utilities;

/// <summary>
/// Provides <see cref="string"/> extensions.
/// </summary>
internal static class StringSerializer
{
    private const string NegativeIdentifier = "-";
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
    public static string Serialize(params string?[]? data)
    {
        if (data == null)
        {
            return NullIdentifier;
        }
        if (data.Length == 0)
        {
            return EmptyIdentifier;
        }
        return Serialize(0, data.Length, data);
    }

    /// <summary>
    /// Serializes an array of <see cref="string"/>.
    /// </summary>
    /// <param name="startIndex">
    /// The index of <paramref name="data"/> to start serialize.
    /// </param>
    /// <param name="count">
    /// The count of <paramref name="data"/> to serialize.
    /// </param>
    /// <param name="data">
    /// The array of string to serialize.
    /// </param>
    /// <returns>
    /// The serialized value of the provided <paramref name="data"/> parameter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="startIndex"/> + <paramref name="count"/> is greater than to <paramref name="data"/> length.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex"/> or <paramref name="count"/> is below zero.
    /// </exception>
    public static string Serialize(int startIndex, int count, params string?[] data)
    {
        if (data == null)
        {
            ArgumentNullException.ThrowIfNull(data);
        }
        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        if (startIndex + count > data.Length)
        {
            throw new ArgumentException(nameof(count) + " is greater than to " + nameof(data) + " length.");
        }
        if (data.Length == 0)
        {
            return EmptyIdentifier;
        }
        string dataLength = CompressNumber(count);
        string[] lengths = new string[count];
        int maxDigitLength = dataLength.Length;
        for (int i = 0; i < count; i++)
        {
            string? currentData = data[startIndex + i];
            if (currentData == null)
            {
                lengths[i] = NullIdentifier;
            }
            else if (string.IsNullOrEmpty(currentData))
            {
                lengths[i] = EmptyIdentifier;
            }
            else
            {
                lengths[i] = CompressNumber(currentData.Length);
            }
            if (maxDigitLength < lengths[i].Length)
            {
                maxDigitLength = lengths[i].Length;
            }
        }
        for (int i = 0; i < count; i++)
        {
            lengths[i] = lengths[i].PadLeft(maxDigitLength, Base62Charset[0]);
        }
        string[] serialized = new string[lengths.Length + count + 2];
        serialized[0] = CompressNumber(maxDigitLength);
        serialized[1] = dataLength.PadLeft(maxDigitLength, Base62Charset[0]);
        Array.Copy(lengths, 0, serialized, 2, lengths.Length);
        Array.Copy(data, startIndex, serialized, lengths.Length + 2, count);
        return string.Join("", serialized);
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
    public static string?[]? Deserialize(string? data)
    {
        if (data == null) return null;
        if (data.Equals(NullIdentifier)) return null;
        if (data.Equals(EmptyIdentifier)) return Array.Empty<string>();
        if (data.Length < 4) return Array.Empty<string>();

        int indexDigits = (int)ExtractNumber(data[0].ToString());
        int indexCount = (int)ExtractNumber(data.Substring(1, indexDigits));
        var indices = data.Substring(1 + indexDigits, indexDigits * indexCount);
        var dataPart = data[(1 + indexDigits + (indexDigits * indexCount))..];
        string?[] datas = new string[indexCount];
        var currIndex = 0;
        for (int i = 0; i < indexCount; i++)
        {
            var subData = indices.Substring(indexDigits * i, indexDigits).TrimStart(Base62Charset[0]);
            if (subData.Equals(NullIdentifier)) datas[i] = null;
            else if (subData.Equals(EmptyIdentifier)) datas[i] = "";
            else
            {
                int currLength = (int)ExtractNumber(subData);
                datas[i] = dataPart.Substring(currIndex, currLength);
                currIndex += currLength;
            }
        }
        return datas;
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
    /// <exception cref="ArgumentException">
    /// Throws when the provided <paramref name="number"/> is not from a base62 value.
    /// </exception>
    public static long ExtractNumber(string number)
    {
        int[] indexes = new int[number.Length];
        if (!string.IsNullOrEmpty(number))
        {
            int floorLoop = 0;
            if (number[0] == NegativeIdentifier[0])
            {
                indexes[0] = -1;
                floorLoop = 1;
            }
            for (int i = floorLoop; i < number.Length; i++)
            {
                int indexOf = Base62Charset.IndexOf(number[i]);
                if (indexOf < 0)
                {
                    throw new ArgumentException("The number is not a base62 value.");
                }
                indexes[i] = indexOf;
            }
        }
        return NumberSerializer.ToNormalBaseSystem(indexes, 62);
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
    public static string CompressNumber(long number)
    {
        int[] arbitraryBase = NumberSerializer.ToArbitraryBaseSystem(number, 62);
        string str = "";
        int floorLoop = 0;
        if (arbitraryBase[0] < 0)
        {
            str += NegativeIdentifier;
            floorLoop = 1;
        }
        for (int i = floorLoop; i < arbitraryBase.Length; i++)
        {
            str += Base62Charset[arbitraryBase[i]];
        }
        return str;
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

        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            CopyTo(msi, gs);
        }
        return mso.ToArray();
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
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            CopyTo(gs, mso);
        }
        return Encoding.UTF8.GetString(mso.ToArray());
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
