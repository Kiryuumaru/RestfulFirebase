using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Data;
using RestfulFirebase.Common.Internals;
using ObservableHelpers.ComponentModel;
using System.Collections;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Abstractions;

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
        return GetPropertyName(propertyInfo.Name);
    }

    public static string GetFieldName(string propertyName)
    {
        return $"{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName[1..]}";
    }

    public static PropertyInfo? GetPropertyInfo([DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields)] Type objType, string documentFieldName, JsonSerializerOptions? jsonSerializerOptions)
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

    public static TypedDocumentFieldPair? GetDocumentField(
        [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields)] Type objType, Type? propertyType, string propertyName, JsonSerializerOptions? jsonSerializerOptions)
    {
        PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

        return GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, propertyType, propertyName, jsonSerializerOptions);
    }

    public static TypedDocumentFieldPair? GetDocumentField(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, Type? propertyType, string propertyName, JsonSerializerOptions? jsonSerializerOptions)
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

        if (propertyType != null &&
            propertyType == propertyInfo.PropertyType)
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

    public static TypedDocumentFieldPair[] GetDocumentFieldPath(
        [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.NonPublicProperties |
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.NonPublicFields)] Type objType, Type? propertyType, string[] propertyNamePath, JsonSerializerOptions? jsonSerializerOptions)
    {
        if (!propertyNamePath.Any())
        {
            throw new ArgumentException($"{propertyNamePath} is empty.");
        }

        List<TypedDocumentFieldPair> documentFields = new();

        Type currentType = objType;
        for (int i = 0; i < propertyNamePath.Length; i++)
        {
            if (i >= propertyNamePath.Length - 1)
            {
                var documentField = GetDocumentField(currentType, propertyType, propertyNamePath[i], jsonSerializerOptions);
                if (documentField == null)
                {
                    throw new ArgumentException($"\"{currentType}\" does not have a writable property \"{propertyNamePath[i]}\"");
                }
                documentFields.Add(documentField);
                currentType = documentField.Type;
            }
            else
            {
                var documentField = GetDocumentField(currentType, null, propertyNamePath[i], jsonSerializerOptions);
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
