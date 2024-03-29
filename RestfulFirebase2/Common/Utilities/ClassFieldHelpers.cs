﻿using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using RestfulFirebase.Common.Internals;
using ObservableHelpers.ComponentModel;

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

    public static PropertyInfo? GetPropertyInfo([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, string documentFieldName, JsonSerializerOptions? jsonSerializerOptions)
    {
        PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

        return GetPropertyInfo(propertyInfos, fieldInfos, includeOnlyWithAttribute, documentFieldName, jsonSerializerOptions);
    }

    public static PropertyInfo? GetPropertyInfo(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, string documentFieldName, JsonSerializerOptions? jsonSerializerOptions)
    {
        bool checkProperty(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute)
        {
            string? nameToCompare = null;
            bool isValueIncluded = false;

            // Special exclude
            if (propertyInfo.Name == nameof(ObservableObject.SyncOperation) ||
                propertyInfo.Name == nameof(ObservableObject.SynchronizePropertyChangedEvent) ||
                propertyInfo.Name == nameof(ObservableObject.SynchronizePropertyChangingEvent))
            {
                return false;
            }

            if (!propertyInfo.CanWrite)
            {
                return false;
            }

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

            if (nameToCompare == null || string.IsNullOrWhiteSpace(nameToCompare))
            {
                nameToCompare = jsonSerializerOptions?.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
            }

            return nameToCompare.Equals(documentFieldName);
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

            string propertyNameEquivalent = GetPropertyName(fieldInfo);

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

    public static TypedDocumentFieldPair? GetDocumentField([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, string propertyName, JsonSerializerOptions? jsonSerializerOptions)
    {
        PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

        return GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, propertyName, jsonSerializerOptions);
    }

    public static TypedDocumentFieldPair? GetDocumentField(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, string propertyName, JsonSerializerOptions? jsonSerializerOptions)
    {
        TypedDocumentFieldPair? getDocumentField(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute)
        {
            string? documentFieldName = null;
            bool isValueIncluded = false;

            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            if (memberToCheckAttribute.GetCustomAttribute(typeof(FirebaseValueAttribute)) is FirebaseValueAttribute firebaseValueAttribute)
            {
                documentFieldName = firebaseValueAttribute.Name;
                isValueIncluded = true;
            }
            else if (!includeOnlyWithAttribute)
            {
                if (memberToCheckAttribute.GetCustomAttribute(typeof(JsonPropertyNameAttribute)) is JsonPropertyNameAttribute jsonPropertyNameAttribute)
                {
                    documentFieldName = jsonPropertyNameAttribute.Name;
                }
                isValueIncluded = true;
            }

            if (!isValueIncluded)
            {
                return null;
            }

            if (documentFieldName == null || string.IsNullOrWhiteSpace(documentFieldName))
            {
                documentFieldName = jsonSerializerOptions?.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
            }

            return new(propertyInfo.PropertyType, documentFieldName);
        }

        PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(i => i.Name.Equals(propertyName));

        if (propertyInfo == null)
        {
            return null;
        }

        TypedDocumentFieldPair? fromProperty = getDocumentField(propertyInfo, propertyInfo);

        if (fromProperty == null)
        {
            string equivalentFieldName = GetFieldName(propertyInfo);
            FieldInfo? fieldInfo = fieldInfos.FirstOrDefault(i => i.Name.Equals(equivalentFieldName));

            if (fieldInfo != null)
            {
                return getDocumentField(propertyInfo, fieldInfo);
            }
            else
            {
                return null;
            }
        }

        return fromProperty;
    }

    public static TypedDocumentFieldPair[] GetDocumentFieldPath([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, string[] propertyNamePath, JsonSerializerOptions? jsonSerializerOptions)
    {
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

        if (!propertyNamePath.Any())
        {
            throw new ArgumentException($"{propertyNamePath} is empty.");
        }

        List<TypedDocumentFieldPair> documentFields = new();

        Type currentType = objType;
        for (int i = 0; i < propertyNamePath.Length; i++)
        {
            if (getDictionaryValueType(currentType) is Type dictionaryValueType)
            {
                documentFields.Add(new TypedDocumentFieldPair(dictionaryValueType, propertyNamePath[i]));
                currentType = dictionaryValueType;
            }
            else
            {
                var documentField = GetDocumentField(currentType, propertyNamePath[i], jsonSerializerOptions);
                if (documentField == null)
                {
                    throw new ArgumentException($"\"{currentType}\" does not have a writable property \"{propertyNamePath[i]}\"");
                }
                documentFields.Add(documentField);
                currentType = documentField.Type;
            }
        }

        return documentFields.ToArray();
    }
}
