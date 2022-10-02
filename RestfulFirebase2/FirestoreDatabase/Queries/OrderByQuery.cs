using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;
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
        /// Creates an instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Ascending"/> order.
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
            orderByQuery.Add(new(propertyName, Direction.Ascending));
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Descending"/> order.
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
            orderByQuery.Add(new(propertyName, Direction.Descending));
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Ascending"/> document name order.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        public Builder AscendingDocumentName()
        {
            orderByQuery.Add(new(DocumentFieldHelpers.DocumentName, Direction.Ascending));
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderByQuery"/> with <see cref="Direction.Descending"/> document name order.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        public Builder DescendingDocumentName()
        {
            orderByQuery.Add(new(DocumentFieldHelpers.DocumentName, Direction.Descending));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.OrderByQuery"/> to the builder.
        /// </summary>
        /// <param name="propertyName">
        /// The order based on the property name of the model to order.
        /// </param>
        /// <param name="orderDirection">
        /// The <see cref="Enums.Direction"/> of the order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "orderBy" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Add(string propertyName, Direction orderDirection)
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
    /// Gets or sets the <see cref="Enums.Direction"/> of the order.
    /// </summary>
    public Direction OrderDirection { get; set; }

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

    /// <inheritdoc cref="Builder.AscendingDocumentName()"/>
    public static Builder AscendingDocumentName()
    {
        return new Builder().AscendingDocumentName();
    }

    /// <inheritdoc cref="Builder.DescendingDocumentName()"/>
    public static Builder DescendingDocumentName()
    {
        return new Builder().DescendingDocumentName();
    }

    /// <inheritdoc cref="Builder.Add(string, Direction)"/>
    public static Builder Add(string propertyName, Direction orderDirection)
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
    /// The <see cref="Enums.Direction"/> of the order.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public OrderByQuery(string propertyName, Direction orderDirection)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        PropertyName = propertyName;
        OrderDirection = orderDirection;
    }
}
