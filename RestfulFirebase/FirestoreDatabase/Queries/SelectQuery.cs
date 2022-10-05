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
        public bool IsDocumentNameOnly => SelectQuery.Any(i => i.NamePath.Any(j => j == DocumentFieldHelpers.DocumentName));

        /// <summary>
        /// Adds the <see cref="Queries.SelectQuery"/> to the builder.
        /// </summary>
        /// <param name="namePath">
        /// The property name to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "select" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="namePath"/> is a null reference.
        /// </exception>
        public Builder Add(params string[] namePath)
        {
            if (IsDocumentNameOnly)
            {
                throw new ArgumentException("Select query is set to return only the document name.");
            }

            selectQuery.Add(new(namePath));
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

            selectQuery.Add(new(new string[] { DocumentFieldHelpers.DocumentName }));
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
    /// Gets the path of the property or field.
    /// </summary>
    public string[] NamePath { get; }

    internal SelectQuery(string[] namePath)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        NamePath = namePath;
    }
}
