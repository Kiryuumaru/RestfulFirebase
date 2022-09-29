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
/// The "from" parameter for query.
/// </summary>
public class FromQuery
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Queries.FromQuery"/></para>
    /// <para><see cref="Queries.FromQuery"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Queries.FromQuery"/></para>
    /// <para><see cref="References.CollectionReference"/></para>
    /// <para><see cref="References.CollectionReference"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="References.CollectionReference"/></para>
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
        /// Gets the list of <see cref="Queries.FromQuery"/>.
        /// </summary>
        public List<FromQuery> FromQuery { get; } = new();

        /// <summary>
        /// Adds the <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="collectionReference">
        /// The <see cref="References.CollectionReference"/>. When set, selects only collections with this ID.
        /// </param>
        /// <param name="allDescendants">
        /// <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder Add(CollectionReference collectionReference, bool allDescendants = false)
        {
            FromQuery.Add(Queries.FromQuery.Create(collectionReference, allDescendants));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder Add(FromQuery orderBy)
        {
            FromQuery.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.FromQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with added order.
        /// </returns>
        public Builder AddRange(IEnumerable<FromQuery> orderBy)
        {
            FromQuery.AddRange(orderBy);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> to convert.
        /// </param>
        public static implicit operator Builder(FromQuery orderBy)
        {
            return Create().Add(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> array to convert.
        /// </param>
        public static implicit operator Builder(FromQuery[] orderBy)
        {
            return Create().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> list to convert.
        /// </param>
        public static implicit operator Builder(List<FromQuery> orderBy)
        {
            return Create().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="collectionReference">
        /// The <see cref="References.CollectionReference"/> to convert.
        /// </param>
        public static implicit operator Builder(CollectionReference collectionReference)
        {
            return Create().Add(collectionReference);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="collectionReferences">
        /// The <see cref="References.CollectionReference"/> array to convert.
        /// </param>
        public static implicit operator Builder(CollectionReference[] collectionReferences)
        {
            return Create().AddRange(collectionReferences.Select(i => Queries.FromQuery.Create(i)));
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="collectionReferences">
        /// The <see cref="References.CollectionReference"/> list to convert.
        /// </param>
        public static implicit operator Builder(List<CollectionReference> collectionReferences)
        {
            return Create().AddRange(collectionReferences.Select(i => Queries.FromQuery.Create(i)));
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="References.CollectionReference"/>. When set, selects only collections with this ID.
    /// </summary>
    public CollectionReference CollectionReference { get; set; }

    /// <summary>
    /// Gets or sets <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </summary>
    public bool AllDescendants { get; set; }

    /// <summary>
    /// Creates an instance of <see cref="FromQuery"/>.
    /// </summary>
    /// <param name="collectionReference">
    /// The <see cref="References.CollectionReference"/>. When set, selects only collections with this ID.
    /// </param>
    /// <param name="allDescendants">
    /// <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </param>
    /// <returns>
    /// The created <see cref="FromQuery"/>
    /// </returns>
    public static FromQuery Create(CollectionReference collectionReference, bool allDescendants = false)
    {
        return new(collectionReference, allDescendants);
    }

    /// <summary>
    /// Creates an instance of <see cref="FromQuery"/>.
    /// </summary>
    /// <param name="collectionId">
    /// The collection ID. When set, selects only collections with this ID.
    /// </param>
    /// <param name="allDescendants">
    /// <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </param>
    /// <returns>
    /// The created <see cref="FromQuery"/>
    /// </returns>
    public static FromQuery Create(string collectionId, bool allDescendants = false)
    {
        return new(CollectionReference.Create(collectionId), allDescendants);
    }

    internal FromQuery(CollectionReference collectionReference, bool allDescendants)
    {
        ArgumentNullException.ThrowIfNull(collectionReference);

        CollectionReference = collectionReference;
        AllDescendants = allDescendants;
    }
}
