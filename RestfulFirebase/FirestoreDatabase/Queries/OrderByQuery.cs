﻿using RestfulFirebase.FirestoreDatabase.Enums;
using System.Collections.Generic;

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
        private readonly List<OrderByQuery> orderByQuery = new();

        /// <summary>
        /// Gets the list of <see cref="Queries.OrderByQuery"/>.
        /// </summary>
        public IReadOnlyList<OrderByQuery> OrderByQuery { get; }

        internal Builder()
        {
            OrderByQuery = orderByQuery.AsReadOnly();
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderByQuery"/> with <see cref="OrderDirection.Ascending"/> order.
        /// </summary>
        /// <param name="propertyName">
        /// The order based on the property name of the model to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Ascending(string propertyName)
        {
            orderByQuery.Add(new(propertyName, OrderDirection.Ascending));
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderByQuery"/> with <see cref="OrderDirection.Descending"/> order.
        /// </summary>
        /// <param name="propertyName">
        /// The order based on the property name of the model to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Descending(string propertyName)
        {
            orderByQuery.Add(new(propertyName, OrderDirection.Descending));
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
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Add(string propertyName, OrderDirection orderDirection)
        {
            orderByQuery.Add(new(propertyName, orderDirection));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.OrderByQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderByQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder Add(OrderByQuery orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            orderByQuery.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.OrderByQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.OrderByQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<OrderByQuery> orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            orderByQuery.AddRange(orderBy);
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
            return new Builder().Add(orderBy);
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
            return new Builder().AddRange(orderBy);
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
            return new Builder().AddRange(orderBy);
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

    /// <inheritdoc cref="Builder.Ascending(string)"/>
    public static Builder Ascending(string propertyName)
    {
        return new Builder().Ascending(propertyName);
    }

    /// <inheritdoc cref="Builder.Descending(string)"/>
    public static Builder Descending(string propertyName)
    {
        return new Builder().Descending(propertyName);
    }

    /// <inheritdoc cref="Builder.Add(string, OrderDirection)"/>
    public static Builder Add(string propertyName, OrderDirection orderDirection)
    {
        return new Builder().Add(propertyName, orderDirection);
    }

    /// <inheritdoc cref="Builder.Add(OrderByQuery)"/>
    public static Builder Add(OrderByQuery orderBy)
    {
        return new Builder().Add(orderBy);
    }

    /// <inheritdoc cref="Builder.AddRange(IEnumerable{OrderByQuery})"/>
    public static Builder Add(IEnumerable<OrderByQuery> orderBy)
    {
        return new Builder().AddRange(orderBy);
    }

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
    public OrderByQuery(string propertyName, OrderDirection orderDirection)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        PropertyName = propertyName;
        OrderDirection = orderDirection;
    }
}
