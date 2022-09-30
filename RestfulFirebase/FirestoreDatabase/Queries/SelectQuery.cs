using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The "select" parameter for query.
/// </summary>
public class SelectQuery
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Queries.SelectQuery"/></para>
    /// <para><see cref="Queries.SelectQuery"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Queries.SelectQuery"/></para>
    /// </summary>
    public class Builder
    {
        private readonly List<SelectQuery> selectQuery = new();

        /// <summary>
        /// Gets the list of <see cref="Queries.SelectQuery"/>.
        /// </summary>
        public IReadOnlyList<SelectQuery> SelectQuery { get; }

        internal Builder()
        {
            SelectQuery = selectQuery.AsReadOnly();
        }

        /// <summary>
        /// Gets <c>true</c> whether the "select" query is set to return the document name only; otherwise, <c>false</c>.
        /// </summary>
        public bool IsDocumentNameOnly => SelectQuery.Any(i => i.PropertyName == DocumentFieldHelpers.DocumentName);

        /// <summary>
        /// Adds the <see cref="Queries.SelectQuery"/> to the builder.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "select" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Add(string propertyName)
        {
            if (IsDocumentNameOnly)
            {
                throw new ArgumentException("Select query is set to return only the document name.");
            }

            selectQuery.Add(new(propertyName));
            return this;
        }

        /// <summary>
        /// Adds the '__name__' to the <see cref="Queries.SelectQuery"/> builder to only return the name of the document.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/> with new added "select" query.
        /// </returns>
        public Builder DocumentNameOnly()
        {
            if (SelectQuery.Count != 0)
            {
                throw new ArgumentException("Select query already contains field projections.");
            }

            selectQuery.Add(new(DocumentFieldHelpers.DocumentName));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.SelectQuery"/> to the builder.
        /// </summary>
        /// <param name="select">
        /// The <see cref="Queries.SelectQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "select" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="select"/> is a null reference.
        /// </exception>
        public Builder Add(SelectQuery select)
        {
            ArgumentNullException.ThrowIfNull(select);
            if (IsDocumentNameOnly)
            {
                throw new ArgumentException("Select query is set to return only the document name.");
            }

            selectQuery.Add(select);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.SelectQuery"/> to the builder.
        /// </summary>
        /// <param name="filter">
        /// The multiple of <see cref="Queries.SelectQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "select" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<SelectQuery> filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            if (IsDocumentNameOnly)
            {
                throw new ArgumentException("Select query is set to return only the document name.");
            }

            selectQuery.AddRange(filter);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.SelectQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.SelectQuery"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(SelectQuery filter)
        {
            return new Builder().Add(filter);
        }

        /// <summary>
        /// Converts the <see cref="Queries.SelectQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.SelectQuery"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(SelectQuery[] filter)
        {
            return new Builder().AddRange(filter);
        }

        /// <summary>
        /// Converts the <see cref="Queries.SelectQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.SelectQuery"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<SelectQuery> filter)
        {
            return new Builder().AddRange(filter);
        }
    }

    /// <inheritdoc cref="Builder.Add(string)"/>
    public static Builder Add(string propertyName)
    {
        return new Builder().Add(propertyName);
    }

    /// <inheritdoc cref="Builder.DocumentNameOnly()"/>
    public static Builder DocumentNameOnly()
    {
        return new Builder().DocumentNameOnly();
    }

    /// <inheritdoc cref="Builder.Add(SelectQuery)"/>
    public static Builder Add(SelectQuery select)
    {
        return new Builder().Add(select);
    }

    /// <summary>
    /// Gets or sets the property name to which to apply the operator.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Creates new instance of <see cref="SelectQuery"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The property name of document's fields to return.
    /// </param>
    /// <returns>
    /// The created <see cref="SelectQuery"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public SelectQuery(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        PropertyName = propertyName;
    }
}
