using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The order and cursor parameter for query.
/// </summary>
public class OrderQuery
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Queries.OrderQuery"/></para>
    /// <para><see cref="Queries.OrderQuery"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Queries.OrderQuery"/></para>
    /// </summary>
    public class Builder
    {
        private readonly List<OrderQuery> orderQuery = new();

        /// <summary>
        /// Gets the list of <see cref="Queries.OrderQuery"/>.
        /// </summary>
        public IReadOnlyList<OrderQuery> OrderQuery { get; }

        /// <summary>
        /// Gets or sets <c>true</c> whether the position is on the given start values, relative to the sort order defined by the query; otherwise, <c>false</c> to skip the given start values.
        /// </summary>
        public bool IsStartAfter { get; set; }

        /// <summary>
        /// Gets or sets <c>true</c> whether the position is on the given end values, relative to the sort order defined by the query; otherwise, <c>false</c> to skip the given end values.
        /// </summary>
        public bool IsEndBefore { get; set; }

        internal Builder()
        {
            OrderQuery = orderQuery.AsReadOnly();
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderQuery"/> with <see cref="Direction.Ascending"/> order.
        /// </summary>
        /// <param name="namePath">
        /// The order based on the name path of the model to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="namePath"/> is a null reference.
        /// </exception>
        public Builder Ascending(params string[] namePath)
        {
            orderQuery.Add(new(namePath, Direction.Ascending, null, null));
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderQuery"/> with <see cref="Direction.Descending"/> order.
        /// </summary>
        /// <param name="namePath">
        /// The order based on the name path of the model to order.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="namePath"/> is a null reference.
        /// </exception>
        public Builder Descending(params string[] namePath)
        {
            orderQuery.Add(new(namePath, Direction.Descending, null, null));
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderQuery"/> with <see cref="Direction.Ascending"/> document name order.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        public Builder AscendingDocumentName()
        {
            orderQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, Direction.Ascending, null, null));
            return this;
        }

        /// <summary>
        /// Creates an instance of <see cref="Queries.OrderQuery"/> with <see cref="Direction.Descending"/> document name order.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        public Builder DescendingDocumentName()
        {
            orderQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }, Direction.Descending, null, null));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.OrderQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder Add(OrderQuery orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            orderQuery.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.OrderQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.OrderQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<OrderQuery> orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            orderQuery.AddRange(orderBy);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderQuery"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(OrderQuery orderBy)
        {
            return new Builder().Add(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderQuery"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(OrderQuery[] orderBy)
        {
            return new Builder().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.OrderQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.OrderQuery"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<OrderQuery> orderBy)
        {
            return new Builder().AddRange(orderBy);
        }
    }

    /// <summary>
    /// Gets the path of the property or field.
    /// </summary>
    public string[] NamePath { get; }

    /// <summary>
    /// Gets or sets the <see cref="Enums.Direction"/> of the order.
    /// </summary>
    public Direction Direction { get; set; }

    /// <summary>
    /// Gets or sets the value that represent a start position, in the order they appear in the order by clause of a query.
    /// </summary>
    public object? StartPosition { get; set; }

    /// <summary>
    /// Gets or sets the value that represent a end position, in the order they appear in the order by clause of a query.
    /// </summary>
    public object? EndPosition { get; set; }

    internal OrderQuery(string[] namePath, Direction direction, object? startPosition, object? endPosition)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        NamePath = namePath;
        Direction = direction;
        StartPosition = startPosition;
        EndPosition = endPosition;
    }
}
