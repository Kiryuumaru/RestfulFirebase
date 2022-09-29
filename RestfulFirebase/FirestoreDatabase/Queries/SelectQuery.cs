using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;

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
        /// Gets the list of <see cref="Queries.SelectQuery"/>.
        /// </summary>
        public List<SelectQuery> SelectQuery { get; } = new();

        /// <summary>
        /// Gets or sets <c>true</c> to only return the name of the document; otherwise, <c>false</c>.
        /// </summary>
        public bool DocumentNameOnly { get; set; }

        /// <summary>
        /// Adds the <see cref="Queries.SelectQuery"/> to the builder.
        /// </summary>
        /// <param name="propertyName">
        /// The property name to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added select query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName"/> is a null reference.
        /// </exception>
        public Builder Add(string propertyName)
        {
            ArgumentNullException.ThrowIfNull(propertyName);
            if (DocumentNameOnly)
            {
                throw new ArgumentException("Select query is set to return only the document name.");
            }

            SelectQuery.Add(Queries.SelectQuery.Create(propertyName));
            return this;
        }

        /// <summary>
        /// Adds the '__name__' to the <see cref="Queries.SelectQuery"/> builder to only return the name of the document.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/> with added select query.
        /// </returns>
        public Builder DocumentName()
        {
            if (SelectQuery.Count != 0)
            {
                throw new ArgumentException("Select query already contains field projections.");
            }
            DocumentNameOnly = true;
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.SelectQuery"/> to the builder.
        /// </summary>
        /// <param name="filter">
        /// The <see cref="Queries.SelectQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public Builder Add(SelectQuery filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            if (DocumentNameOnly)
            {
                throw new ArgumentException("Select query is set to return only the document name.");
            }

            SelectQuery.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.SelectQuery"/> to the builder.
        /// </summary>
        /// <param name="filter">
        /// The multiple of <see cref="Queries.SelectQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="filter"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<SelectQuery> filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            if (DocumentNameOnly)
            {
                throw new ArgumentException("Select query is set to return only the document name.");
            }

            SelectQuery.AddRange(filter);
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
            return Create().Add(filter);
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
            return Create().AddRange(filter);
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
            return Create().AddRange(filter);
        }
    }

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
    public static SelectQuery Create(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        return new(propertyName);
    }

    /// <summary>
    /// Adds the '__name__' to the <see cref="SelectQuery"/> builder to only return the name of the document.
    /// </summary>
    /// <returns>
    /// The created <see cref="Builder"/>.
    /// </returns>
    public static Builder DocumentName()
    {
        return Builder.Create().DocumentName();
    }

    /// <summary>
    /// Gets or sets the property name to which to apply the operator.
    /// </summary>
    public string PropertyName { get; set; }

    internal SelectQuery(string propertyName)
    {
        PropertyName = propertyName;
    }
}
