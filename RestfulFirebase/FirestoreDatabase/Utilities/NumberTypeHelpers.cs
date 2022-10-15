using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Data;

namespace RestfulFirebase.FirestoreDatabase.Utilities;

internal static class NumberTypeHelpers
{
    internal static NumberType GetNumberType(Type incrementType)
    {
        if (incrementType != typeof(object) &&
            (incrementType.IsAssignableFrom(typeof(sbyte)) ||
            incrementType.IsAssignableFrom(typeof(byte)) ||
            incrementType.IsAssignableFrom(typeof(short)) ||
            incrementType.IsAssignableFrom(typeof(ushort)) ||
            incrementType.IsAssignableFrom(typeof(int)) ||
            incrementType.IsAssignableFrom(typeof(uint)) ||
            incrementType.IsAssignableFrom(typeof(long)) ||
            incrementType.IsAssignableFrom(typeof(ulong)) ||
            incrementType.IsAssignableFrom(typeof(nint)) ||
            incrementType.IsAssignableFrom(typeof(nuint))))
        {
            return NumberType.Integer;
        }
        else if (incrementType != typeof(object) &&
            (incrementType.IsAssignableFrom(typeof(float)) ||
            incrementType.IsAssignableFrom(typeof(double))))
        {
            return NumberType.Double;
        }
        else if (incrementType != typeof(object) &&
            incrementType.IsAssignableFrom(typeof(decimal)))
        {
            throw new ArgumentException("Decimal number is not yet supported.");
        }
        else
        {
            throw new ArgumentException($"\"{incrementType}\" type is not supported.");
        }
    }
}
