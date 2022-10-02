using RestfulFirebase.FirestoreDatabase.Enums;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The "where" parameter for query.
/// </summary>
public abstract class FilterQuery
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Queries.FilterQuery"/></para>
    /// <para><see cref="Queries.FilterQuery"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Queries.FilterQuery"/></para>
    /// </summary>
    public class Builder
    {
        private readonly List<FilterQuery> filterQuery = new();

        /// <summary>
        /// Gets the list of <see cref="Queries.FilterQuery"/>.
        /// </summary>
        public IReadOnlyList<FilterQuery> FilterQuery { get; }

        internal Builder()
        {
            FilterQuery = filterQuery.AsReadOnly();
        }

        /// <summary>
        /// Adds new instance of <see cref="UnaryFilterQuery"/> to the builder.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to which to apply the operator.
        /// </param>
        /// <param name="operator">
        /// The <see cref="UnaryOperator"/> to apply.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "where" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Add(string propertyName, UnaryOperator @operator)
        {
            filterQuery.Add(new UnaryFilterQuery(propertyName, @operator));
            return this;
        }

        /// <summary>
        /// Adds new instance of <see cref="FieldFilterQuery"/> to the builder.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to which to apply the operator.
        /// </param>
        /// <param name="operator">
        /// The <see cref="FieldOperator"/> to apply.
        /// </param>
        /// <param name="value">
        /// The value to compare to.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "where" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Add(string propertyName, FieldOperator @operator, object? value)
        {
            filterQuery.Add(new FieldFilterQuery(propertyName, @operator, value));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.FilterQuery"/> to the builder.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.FilterQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "where" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public Builder Add(FilterQuery filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            filterQuery.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.FilterQuery"/> to the builder.
        /// </summary>
        /// <param name="filter">
        /// The multiple of <see cref="Queries.FilterQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "where" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<FilterQuery> filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            filterQuery.AddRange(filter);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.FilterQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.FilterQuery"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(FilterQuery filter)
        {
            return new Builder().Add(filter);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FilterQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.FilterQuery"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(FilterQuery[] filter)
        {
            return new Builder().AddRange(filter);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FilterQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.FilterQuery"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<FilterQuery> filter)
        {
            return new Builder().AddRange(filter);
        }
    }

    /// <inheritdoc cref="Builder.Add(string, UnaryOperator)"/>
    public static Builder Add(string propertyName, UnaryOperator @operator)
    {
        return new Builder().Add(propertyName, @operator);
    }

    /// <inheritdoc cref="Builder.Add(string, FieldOperator, object?)"/>
    public static Builder Add(string propertyName, FieldOperator @operator, object? value)
    {
        return new Builder().Add(propertyName, @operator, value);
    }

    /// <inheritdoc cref="Builder.Add(FilterQuery)"/>
    public static Builder Add(FilterQuery filter)
    {
        return new Builder().Add(filter);
    }

    /// <inheritdoc cref="Builder.AddRange(IEnumerable{FilterQuery})"/>
    public static Builder AddRange(IEnumerable<FilterQuery> filter)
    {
        return new Builder().AddRange(filter);
    }

    /// <summary>
    /// Gets or sets the property name to which to apply the operator.
    /// </summary>
    public string PropertyName { get; set; }

    internal FilterQuery(string propertyName)
    {
        PropertyName = propertyName;
    }
}
