using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The orderBy parameter for query.
/// </summary>
public class OrderBy
{
    /// <summary>
    /// Gets or sets the order based on the property name of the model to order.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="OrderDirection"/> of the query.
    /// </summary>
    public OrderDirection OrderDirection { get; set;}

    /// <summary>
    /// Creates an instance of <see cref="OrderBy"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <param name="orderDirection">
    /// The <see cref="Enums.OrderDirection"/> of the query.
    /// </param>
    public OrderBy(string propertyName, OrderDirection orderDirection)
    {
        PropertyName = propertyName;
        OrderDirection = orderDirection;
    }

    /// <summary>
    /// Creates a multiple <see cref="OrderBy"/>.
    /// </summary>
    /// <param name="orderBy"></param>
    /// The paramerer for orderBy query.
    /// <returns></returns>
    public static IEnumerable<OrderBy> Create(params (string field, OrderDirection orderDirection)[] orderBy)
    {
        List<OrderBy> orders = new();

        foreach (var (field, orderDirection) in orderBy)
        {
            orders.Add(new OrderBy(field, orderDirection));
        }

        return orders.AsReadOnly();
    }

    internal static string BuildAsQueryParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(IEnumerable<OrderBy> orderBy, JsonSerializerOptions? jsonSerializerOptions)
    {
        return BuildAsQueryParameter(typeof(T), orderBy, jsonSerializerOptions);
    }

    internal static string BuildAsQueryParameter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, IEnumerable<OrderBy> orderBy, JsonSerializerOptions? jsonSerializerOptions)
    {
        PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

        return BuildAsQueryParameter(objType, orderBy, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions);
    }

    internal static string BuildAsQueryParameter(Type objType, IEnumerable<OrderBy> orderBy, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, JsonSerializerOptions? jsonSerializerOptions)
    {
        List<string> orderByQuery = new();

        foreach (var order in orderBy)
        {
            orderByQuery.Add($"{order.GetDocumentFieldName(objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions)} {(order.OrderDirection == OrderDirection.Ascending ? "asc" : "desc")}");
        }

        return string.Join(",", orderByQuery);
    }

    internal string GetDocumentFieldName(Type objType, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, JsonSerializerOptions? jsonSerializerOptions)
    {
        var documentField = ClassMemberHelpers.GetDocumentField(propertyInfos, fieldInfos, includeOnlyWithAttribute, null, PropertyName, jsonSerializerOptions);

        if (documentField == null)
        {
            throw new ArgumentException($"OrderBy property name {PropertyName} does not exist in the model {objType.Name}.");
        }

        return documentField.DocumentFieldName;
    }
}
