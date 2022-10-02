using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using RestfulFirebase.Common.Internals;
using ObservableHelpers.ComponentModel;
using System.Collections;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Utilities;

internal static class DocumentFieldHelpers
{
    internal const string DocumentName = "__name__";

    internal static object GetLimitValue(Type type, bool isMaxValue)
    {
        if (type.IsAssignableFrom(typeof(bool)))
        {
            return isMaxValue;
        }
        else if (
            type.IsAssignableFrom(typeof(sbyte)) ||
            type.IsAssignableFrom(typeof(byte)) ||
            type.IsAssignableFrom(typeof(short)) ||
            type.IsAssignableFrom(typeof(ushort)) ||
            type.IsAssignableFrom(typeof(int)) ||
            type.IsAssignableFrom(typeof(uint)) ||
            type.IsAssignableFrom(typeof(long)) ||
            type.IsAssignableFrom(typeof(ulong)) ||
            type.IsAssignableFrom(typeof(nint)) ||
            type.IsAssignableFrom(typeof(nuint)))
        {
            return isMaxValue ? long.MaxValue : long.MinValue;
        }
        else if (
            type.IsAssignableFrom(typeof(float)) ||
            type.IsAssignableFrom(typeof(double)))
        {
            return isMaxValue ? double.MaxValue : double.MinValue;
        }
        else if (type.IsAssignableFrom(typeof(decimal)))
        {
            return isMaxValue ? decimal.MaxValue : decimal.MinValue;
        }
        else if (
            type.IsAssignableFrom(typeof(DateTime)) ||
            type.IsAssignableFrom(typeof(DateTimeOffset)))
        {
            return isMaxValue ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
        }
        else if (
            type.IsAssignableFrom(typeof(string)) ||
            type.IsAssignableFrom(typeof(char)))
        {
            return isMaxValue ? char.MaxValue : char.MinValue;
        }
        else
        {
            throw new NotSupportedException($"\"{type}\" type is not supported.");
        }
    }
}
