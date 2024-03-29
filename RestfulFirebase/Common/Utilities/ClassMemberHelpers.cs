﻿using System.Globalization;
using System.Reflection;

namespace RestfulFirebase.Common.Utilities;

internal class ClassMemberHelpers
{
    public static string GetPropertyName(FieldInfo fieldInfo)
    {
        return GetPropertyName(fieldInfo.Name);
    }

    public static string GetPropertyName(string fieldName)
    {
        if (fieldName.StartsWith("m_"))
        {
            fieldName = fieldName[2..];
        }
        else if (fieldName.StartsWith("_"))
        {
            fieldName = fieldName.TrimStart('_');
        }

        return $"{char.ToUpper(fieldName[0], CultureInfo.InvariantCulture)}{fieldName[1..]}";
    }

    public static string GetFieldName(PropertyInfo propertyInfo)
    {
        return GetFieldName(propertyInfo.Name);
    }

    public static string GetFieldName(string propertyName)
    {
        return $"{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName[1..]}";
    }
}
