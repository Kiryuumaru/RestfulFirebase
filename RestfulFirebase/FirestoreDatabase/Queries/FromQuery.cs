using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;
using System.Linq;

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
        private readonly List<FromQuery> fromQuery = new();

        /// <summary>
        /// Gets the list of <see cref="Queries.FromQuery"/>.
        /// </summary>
        public IReadOnlyList<FromQuery> FromQuery { get; }

        internal Builder()
        {
            FromQuery = fromQuery.AsReadOnly();
        }

        /// <summary>
        /// Adds an instance of <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="collectionId">
        /// The collection ID. When set, selects only collections with this ID.
        /// </param>
        /// <param name="allDescendants">
        /// <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "from" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionId"/> is a null reference.
        /// </exception>
        public Builder Add(string collectionId, bool allDescendants = false)
        {
            fromQuery.Add(new(collectionId, allDescendants));
            return this;
        }

        /// <summary>
        /// Adds an instance of <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="collectionReference">
        /// The <see cref="References.CollectionReference"/>. When set, selects only collections with this ID.
        /// </param>
        /// <param name="allDescendants">
        /// <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "from" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionReference"/> is a null reference.
        /// </exception>
        public Builder Add(CollectionReference collectionReference, bool allDescendants = false)
        {
            fromQuery.Add(new(collectionReference, allDescendants));
            return this;
        }

        /// <summary>
        /// Adds an instance of <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "from" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder Add(FromQuery orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            fromQuery.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.FromQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.FromQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added "from" query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<FromQuery> orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            fromQuery.AddRange(orderBy);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(FromQuery orderBy)
        {
            return new Builder().Add(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(FromQuery[] orderBy)
        {
            return new Builder().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.FromQuery"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<FromQuery> orderBy)
        {
            return new Builder().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="collectionReference">
        /// The <see cref="References.CollectionReference"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionReference"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(CollectionReference collectionReference)
        {
            return new Builder().Add(collectionReference);
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="collectionReferences">
        /// The <see cref="References.CollectionReference"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionReferences"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(CollectionReference[] collectionReferences)
        {
            return new Builder().AddRange(collectionReferences.Select(i => new FromQuery(i)));
        }

        /// <summary>
        /// Converts the <see cref="Queries.FromQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="collectionReferences">
        /// The <see cref="References.CollectionReference"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionReferences"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<CollectionReference> collectionReferences)
        {
            return new Builder().AddRange(collectionReferences.Select(i => new FromQuery(i)));
        }
    }

    /// <inheritdoc cref="Builder.Add(string, bool)"/>
    public static Builder Add(string collectionId, bool allDescendants = false)
    {
        return new Builder().Add(collectionId, allDescendants);
    }

    /// <inheritdoc cref="Builder.Add(CollectionReference, bool)"/>
    public static Builder Add(CollectionReference collectionReference, bool allDescendants = false)
    {
        return new Builder().Add(collectionReference, allDescendants);
    }

    /// <inheritdoc cref="Builder.Add(FromQuery)"/>
    public static Builder Add(FromQuery orderBy)
    {
        return new Builder().Add(orderBy);
    }

    /// <inheritdoc cref="Builder.AddRange(IEnumerable{FromQuery})"/>
    public static Builder AddRange(IEnumerable<FromQuery> orderBy)
    {
        return new Builder().AddRange(orderBy);
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
    /// <param name="collectionId">
    /// The collection ID. When set, selects only collections with this ID.
    /// </param>
    /// <param name="allDescendants">
    /// <c>true</c> whether to select all descendant collections; otherwise, <c>false</c> to select only collections that are immediate children of the parent specified in the containing request. 
    /// </param>
    /// <returns>
    /// The created <see cref="FromQuery"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionId"/> is a null reference.
    /// </exception>
    public FromQuery(string collectionId, bool allDescendants = false)
    {
        ArgumentNullException.ThrowIfNull(collectionId);

        CollectionReference = new CollectionReference(null, collectionId);
        AllDescendants = allDescendants;
    }

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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionReference"/> is a null reference.
    /// </exception>
    public FromQuery(CollectionReference collectionReference, bool allDescendants = false)
    {
        ArgumentNullException.ThrowIfNull(collectionReference);

        CollectionReference = collectionReference;
        AllDescendants = allDescendants;
    }
}
