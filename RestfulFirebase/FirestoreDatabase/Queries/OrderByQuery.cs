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
/// The "orderBy" parameter for query.
/// </summary>
public class OrderByQuery
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Queries.OrderByQuery"/></para>
    /// <para><see cref="Queries.OrderByQuery"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Queries.OrderByQuery"/></para>
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
        /// Gets the list of <see cref="Queries.OrderByQuery"/>.
        /// </summary>
        public List<OrderByQuery> OrderByQuery { get; } = new();

        /// <summary>
        /// Adds ascending order to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Ascending(string propertyName)
        {
            OrderByQuery.Add(Queries.OrderByQuery.Ascending(propertyName));
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Descending(string propertyName)
        {
            OrderByQuery.Add(Queries.OrderByQuery.Descending(propertyName));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.OrderByQuery"/> to the builder.
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Add(string propertyName, OrderDirection orderDirection)
        {
            OrderByQuery.Add(Queries.OrderByQuery.Create(propertyName, orderDirection));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.OrderByQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderByQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder Add(OrderByQuery orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            OrderByQuery.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.OrderByQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.OrderByQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<OrderByQuery> orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            OrderByQuery.AddRange(orderBy);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderByQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderByQuery"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(OrderByQuery orderBy)
        {
            return Create().Add(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderByQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderByQuery"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(OrderByQuery[] orderBy)
        {
            return Create().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderByQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderByQuery"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<OrderByQuery> orderBy)
        {
            return Create().AddRange(orderBy);
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
    /// Creates an instance of <see cref="OrderByQuery"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <param name="orderDirection">
    /// The <see cref="Enums.OrderDirection"/> of the order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderByQuery"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public static OrderByQuery Create(string propertyName, OrderDirection orderDirection)
    {
        return new(propertyName, orderDirection);
    }

    /// <summary>
    /// Creates an instance of <see cref="OrderDirection.Ascending"/> <see cref="OrderByQuery"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderByQuery"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public static OrderByQuery Ascending(string propertyName)
    {
        return new(propertyName, OrderDirection.Ascending);
    }

    /// <summary>
    /// Creates an instance of <see cref="OrderDirection.Descending"/> <see cref="OrderByQuery"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The order based on the property name of the model to order.
    /// </param>
    /// <returns>
    /// The created <see cref="OrderByQuery"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public static OrderByQuery Descending(string propertyName)
    {
        return new(propertyName, OrderDirection.Descending);
    }

    internal OrderByQuery(string propertyName, OrderDirection orderDirection)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        PropertyName = propertyName;
        OrderDirection = orderDirection;
    }
}
