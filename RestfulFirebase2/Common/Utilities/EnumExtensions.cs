using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace RestfulFirebase.Common.Utilities;

internal static class EnumExtensions
{
    internal static string? ToEnumString<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this T value)
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
