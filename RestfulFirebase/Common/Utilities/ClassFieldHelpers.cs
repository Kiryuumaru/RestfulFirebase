using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Attributes;

namespace RestfulFirebase.Common.Utilities
{
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

        public static PropertyInfo? GetPropertyInfo(Type objType, string documentFieldName, JsonNamingPolicy? jsonNamingPolicy)
        {
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            return GetPropertyInfo(propertyInfos, fieldInfos, includeOnlyWithAttribute, documentFieldName, jsonNamingPolicy);
        }

        public static PropertyInfo? GetPropertyInfo(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, string documentFieldName, JsonNamingPolicy? jsonNamingPolicy)
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
                    nameToCompare = jsonNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
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

        public static string? GetDocumentFieldName(Type objType, string propertyName, JsonNamingPolicy? jsonNamingPolicy)
        {
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            return GetDocumentFieldName(propertyInfos, fieldInfos, includeOnlyWithAttribute, propertyName, jsonNamingPolicy);
        }

        public static string? GetDocumentFieldName(PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, string propertyName, JsonNamingPolicy? jsonNamingPolicy)
        {
            string? getDocumentFieldName(PropertyInfo propertyInfo, MemberInfo memberToCheckAttribute)
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
                    documentFieldName = jsonNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;
                }

                return documentFieldName;
            }

            PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(i => i.Name.Equals(propertyName));

            if (propertyInfo == null)
            {
                return null;
            }

            string? fromProperty = getDocumentFieldName(propertyInfo, propertyInfo);

            if (fromProperty == null)
            {
                string equivalentFieldName = GetFieldName(propertyInfo);
                FieldInfo? fieldInfo = fieldInfos.FirstOrDefault(i => i.Name.Equals(equivalentFieldName));

                if (fieldInfo != null)
                {
                    return getDocumentFieldName(propertyInfo, fieldInfo);
                }
                else
                {
                    return null;
                }
            }

            return fromProperty;
        }
    }
}
