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
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Queries.OrderBy"/></para>
    /// <para><see cref="Queries.OrderBy"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Queries.OrderBy"/></para>
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Creates an instance of <see cref="Builder"/>.
        /// </summary>
        /// <returns>
        /// The created <see cref="Builder"/>.
        /// </returns>
        public static Builder Create()
        {
            return new();
        }

        /// <summary>
        /// Gets the list of <see cref="Queries.OrderBy"/>.
        /// </summary>
        public List<OrderBy> OrderBy { get; } = new();

        /// <summary>
        /// Adds ascending order to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        public Builder Ascending(string propertyName)
        {
            OrderBy.Add(Queries.OrderBy.Ascending(propertyName));
            return this;
        }

        /// <summary>
        /// Adds descending order to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        public Builder Descending(string propertyName)
        {
            OrderBy.Add(Queries.OrderBy.Descending(propertyName));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.OrderBy"/> to the builder.
        /// </summary>
        /// <param name="propertyName">
        /// The order based on the property name of the model to order.
        /// </param>
        /// <param name="orderDirection">
        /// The <see cref="Enums.OrderDirection"/> of the order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder Add(string propertyName, OrderDirection orderDirection)
        {
            OrderBy.Add(Queries.OrderBy.Create(propertyName, orderDirection));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.OrderBy"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderBy"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder Add(OrderBy orderBy)
        {
            OrderBy.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.OrderBy"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.OrderBy"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder AddRange(IEnumerable<OrderBy> orderBy)
        {
            OrderBy.AddRange(orderBy);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderBy"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderBy"/> to convert.
        /// </param>
        public static implicit operator Builder(OrderBy orderBy)
        {
            return Create().Add(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderBy"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderBy"/> array to convert.
        /// </param>
        public static implicit operator Builder(OrderBy[] orderBy)
        {
            return Create().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderBy"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderBy"/> list to convert.
        /// </param>
        public static implicit operator Builder(List<OrderBy> orderBy)
        {
            return Create().AddRange(orderBy);
        }

        internal string BuildAsQueryParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(JsonSerializerOptions? jsonSerializerOptions)
        {
            return BuildAsQueryParameter(typeof(T), jsonSerializerOptions);
        }

        internal string BuildAsQueryParameter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type objType, JsonSerializerOptions? jsonSerializerOptions)
        {
            PropertyInfo[] propertyInfos = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fieldInfos = objType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool includeOnlyWithAttribute = objType.GetCustomAttribute(typeof(FirebaseValueOnlyAttribute)) != null;

            return BuildAsQueryParameter(objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions);
        }

        internal string BuildAsQueryParameter(Type objType, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, bool includeOnlyWithAttribute, JsonSerializerOptions? jsonSerializerOptions)
        {
            List<string> orderByQuery = new();

            foreach (var order in OrderBy)
            {
                orderByQuery.Add($"{order.GetDocumentFieldName(objType, propertyInfos, fieldInfos, includeOnlyWithAttribute, jsonSerializerOptions)} {(order.OrderDirection == OrderDirection.Ascending ? "asc" : "desc")}");
            }

            return string.Join(",", orderByQuery);
        }
    }

    /// <summary>
    /// Gets or sets the order based on the property name of the model to order.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Enums.OrderDirection"/> of the order.
    /// </summary>
    public OrderDirection OrderDirection { get; set; }

    /// <summary>
    /// Creates an instance of <see cref="OrderBy"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <param name="orderDirection">
    /// The <see cref="Enums.OrderDirection"/> of the order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderBy"/>
    /// </returns>
    public static OrderBy Create(string propertyName, OrderDirection orderDirection)
    {
        return new(propertyName, orderDirection);
    }

    /// <summary>
    /// Creates an instance of <see cref="OrderDirection.Ascending"/> <see cref="OrderBy"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderBy"/>
    /// </returns>
    public static OrderBy Ascending(string propertyName)
    {
        return new(propertyName, OrderDirection.Ascending);
    }

    /// <summary>
    /// Creates an instance of <see cref="OrderDirection.Descending"/> <see cref="OrderBy"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderBy"/>
    /// </returns>
    public static OrderBy Descending(string propertyName)
    {
        return new(propertyName, OrderDirection.Descending);
    }

    internal OrderBy(string propertyName, OrderDirection orderDirection)
    {
        PropertyName = propertyName;
        OrderDirection = orderDirection;
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
