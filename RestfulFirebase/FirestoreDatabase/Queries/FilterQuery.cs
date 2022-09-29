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
        /// Gets the list of <see cref="Queries.FilterQuery"/>.
        /// </summary>
        public List<FilterQuery> FilterQuery { get; } = new();

        /// <returns>
        /// The <see cref="Builder"/> added with new filter.
        /// </returns>
        /// <inheritdoc cref="UnaryFilterQuery.Create(string, UnaryOperator)"/>
        public Builder Unary(string propertyName, UnaryOperator @operator)
        {
            FilterQuery.Add(UnaryFilterQuery.Create(propertyName, @operator));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new filter.
        /// </returns>
        /// <inheritdoc cref="FieldFilterQuery.Create(string, FieldOperator, object?)"/>
        public Builder Field(string propertyName, FieldOperator @operator, object? value)
        {
            FilterQuery.Add(FieldFilterQuery.Create(propertyName, @operator, value));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.FilterQuery"/> to the builder.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.FilterQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added filter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public Builder Add(FilterQuery filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            FilterQuery.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.FilterQuery"/> to the builder.
        /// </summary>
        /// <param name="filter">
        /// The multiple of <see cref="Queries.FilterQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added filter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<FilterQuery> filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            FilterQuery.AddRange(filter);
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
            return Create().Add(filter);
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
            return Create().AddRange(filter);
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
            return Create().AddRange(filter);
        }
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
