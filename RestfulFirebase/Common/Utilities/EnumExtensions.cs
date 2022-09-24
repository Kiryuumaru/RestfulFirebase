using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace RestfulFirebase.Common.Utilities;

/// <summary>
/// Provides enum extensions.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Converts enum to its specified string value.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the enum.
    /// </typeparam>
    /// <param name="value">
    /// The enum value to convert.
    /// </param>
    /// <returns>
    /// The converted string of <paramref name="value"/>.
    /// </returns>
    public static string? ToEnumString<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields)] T>(this T value)
    {
        if (value == null)
        {
            ArgumentNullException.ThrowIfNull(value);
        }
        var name = Enum.GetName(typeof(T), value);
        var enumMemberAttribute = ((EnumMemberAttribute[])typeof(T).GetTypeInfo().DeclaredFields.First(f => f.Name == name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();

        return enumMemberAttribute.Value;
    }
}
