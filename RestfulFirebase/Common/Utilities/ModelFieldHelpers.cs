using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using RestfulFirebase.Common.Internals;

namespace RestfulFirebase.Common.Utilities;

internal static class ModelFieldHelpers
{
    public static PropertyInfo? GetPropertyInfo([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, string modelFieldName, JsonSerializerOptions? jsonSerializerOptions)
    {
        PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

        return GetPropertyInfo(propertyInfos, fieldInfos, includeOnlyWithAttribute, modelFieldName, jsonSerializerOptions);
    }

    public static PropertyInfo? GetPropertyInfo(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, string modelFieldName, JsonSerializerOptions? jsonSerializerOptions)
    {
        bool checkProperty(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute)
        {
            string? nameToCompare = null;
            bool isValueIncluded = false;

            // Special exclude (for ObservableHelpers)
            if (propertyInfo.Name == "SyncOperation" ||
                propertyInfo.Name == "SynchronizePropertyChangedEvent" ||
                propertyInfo.Name == "SynchronizePropertyChangingEvent")
            {
                return false;
            }

            if (!propertyInfo.CanWrite)
            {
                return false;
            }

            if (memberToCheckAttribute.GetCustomAttribute(typeof(FirebaseIgnoreAttribute)) is null &&
                memberToCheckAttribute.GetCustomAttribute(typeof(JsonIgnoreAttribute)) is null)
            {
                if (memberToCheckAttribute.GetCustomAttribute(typeof(FirebaseValueAttribute)) is FirebaseValueAttribute firebaseValueAttribute)
                {
                    nameToCompare = firebaseValueAttribute.Name;
                    isValueIncluded = true;
                }
                else if (!includeOnlyWithAttribute)
                {
                    if (memberToCheckAttribute.GetCustomAttribute(typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute jsonPropertyNameAttribute)
                    {
                        nameToCompare = jsonPropertyNameAttribute.Name;
                    }
                    isValueIncluded = true;
                }

                if (!isValueIncluded)
                {
                    return false;
                }
            }

            if (nameToCompare == null || string.IsNullOrWhiteSpace(nameToCompare))
            {
                nameToCompare = jsonSerializerOptions?.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
            }

            return nameToCompare.Equals(modelFieldName);
        }

        foreach (var propertyInfo in propertyInfos)
        {
            if (checkProperty(propertyInfo, propertyInfo))
            {
                return propertyInfo;
            }
        }

        foreach (var fieldInfo in fieldInfos)
        {
            if (fieldInfo.IsStatic)
            {
                continue;
            }

            string propertyNameEquivalent = ClassMemberHelpers.GetPropertyName(fieldInfo);

            PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(i => i.Name.Equals(propertyNameEquivalent));

            if (propertyInfo == null)
            {
                continue;
            }

            if (checkProperty(propertyInfo, fieldInfo))
            {
                return propertyInfo;
            }
        }

        return null;
    }

    public static TypedModelFieldPair? GetModelField([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, string propertyName, JsonSerializerOptions? jsonSerializerOptions)
    {
        PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

        return GetModelField(propertyInfos, fieldInfos, includeOnlyWithAttribute, propertyName, jsonSerializerOptions);
    }

    public static TypedModelFieldPair? GetModelField(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, string propertyName, JsonSerializerOptions? jsonSerializerOptions)
    {
        TypedModelFieldPair? getModelField(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute)
        {
            string? modelFieldName = null;
            bool isValueIncluded = false;

            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            if (memberToCheckAttribute.GetCustomAttribute(typeof(FirebaseIgnoreAttribute)) is null &&
                memberToCheckAttribute.GetCustomAttribute(typeof(JsonIgnoreAttribute)) is null)
            {
                if (memberToCheckAttribute.GetCustomAttribute(typeof(FirebaseValueAttribute)) is FirebaseValueAttribute firebaseValueAttribute)
                {
                    modelFieldName = firebaseValueAttribute.Name;
                    isValueIncluded = true;
                }
                else if (!includeOnlyWithAttribute)
                {
                    if (memberToCheckAttribute.GetCustomAttribute(typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute jsonPropertyNameAttribute)
                    {
                        modelFieldName = jsonPropertyNameAttribute.Name;
                    }
                    isValueIncluded = true;
                }
            }

            if (!isValueIncluded)
            {
                return null;
            }

            if (modelFieldName == null || string.IsNullOrWhiteSpace(modelFieldName))
            {
                modelFieldName = jsonSerializerOptions?.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
            }

            return new(propertyInfo.PropertyType, modelFieldName);
        }

        PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(i => i.Name.Equals(propertyName));

        if (propertyInfo == null)
        {
            return null;
        }

        TypedModelFieldPair? fromProperty = getModelField(propertyInfo, propertyInfo);

        if (fromProperty == null)
        {
            string equivalentFieldName = ClassMemberHelpers.GetFieldName(propertyInfo);
            FieldInfo? fieldInfo = fieldInfos.FirstOrDefault(i => i.Name.Equals(equivalentFieldName));

            if (fieldInfo != null)
            {
                return getModelField(propertyInfo, fieldInfo);
            }
            else
            {
                return null;
            }
        }

        return fromProperty;
    }

    public static TypedModelFieldPair[] GetModelFieldPath([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, string[] propertyNamePath, JsonSerializerOptions? jsonSerializerOptions)
    {
        ArgumentException.ThrowIfHasNullOrEmpty(propertyNamePath);

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] static Type? getDictionaryValueType(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
        {
            var dictionaryInterfaceType = type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (dictionaryInterfaceType != null)
            {
                Type[] dictionaryGenericArgsType = dictionaryInterfaceType.GetGenericArguments();

                return dictionaryGenericArgsType[1];
            }

            return null;
        }

        List<TypedModelFieldPair> modelFields = new();

        Type currentType = objType;
        for (int i = 0; i < propertyNamePath.Length; i++)
        {
            if (getDictionaryValueType(currentType) is Type dictionaryValueType)
            {
                modelFields.Add(new TypedModelFieldPair(dictionaryValueType, propertyNamePath[i]));
                currentType = dictionaryValueType;
            }
            else
            {
                var modelField = GetModelField(currentType, propertyNamePath[i], jsonSerializerOptions);
                if (modelField == null)
                {
                    ArgumentException.Throw($"\"{currentType}\" does not have a writable property \"{propertyNamePath[i]}\"");
                }
                modelFields.Add(modelField);
                currentType = modelField.Type;
            }
        }

        return modelFields.ToArray();
    }

    internal static object GetLimitValue(Type type, bool isMaxValue)
    {
        if (type != typeof(object))
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
        }

        throw new NotSupportedException($"\"{type}\" type is not supported or has no limit values.");
    }
}
