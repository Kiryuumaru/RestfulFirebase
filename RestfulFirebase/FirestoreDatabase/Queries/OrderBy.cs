using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Collections.Generic;
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

    internal static string BuildAsQueryParameter(Type objType, IEnumerable<OrderBy> orderBy, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, JsonNamingPolicy? jsonNamingPolicy)
    {
        List<string> orderByQuery = new();

        foreach (var order in orderBy)
        {
            orderByQuery.Add($"{order.GetDocumentFieldName(objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonNamingPolicy)} {(order.OrderDirection == OrderDirection.Ascending ? "asc" : "desc")}");
        }

        return string.Join(",", orderByQuery);
    }

    internal string GetDocumentFieldName(Type objType, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, JsonNamingPolicy? jsonNamingPolicy)
    {
        string? documentFieldName = ClassMemberHelpers.GetDocumentFieldName(propertyInfos, fieldInfos, includeOnlyWithAttribute, PropertyName, jsonNamingPolicy);

        if (documentFieldName == null)
        {
            throw new ArgumentException($"OrderBy property name {PropertyName} does not exist in the model {objType.Name}.");
        }

        return documentFieldName;
    }
}
