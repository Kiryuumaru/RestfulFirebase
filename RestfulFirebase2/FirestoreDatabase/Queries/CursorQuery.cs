using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The "startAt" or "endAt" parameter for query.
/// </summary>
public class CursorQuery
{
    /// <summary>
    /// The builder for multiple document parameter. Has implicit conversion from
    /// <para><see cref="Queries.CursorQuery"/></para>
    /// <para><see cref="Queries.CursorQuery"/> array</para>
    /// <para><see cref="List{T}"/> with item <see cref="Queries.CursorQuery"/></para>
    /// </summary>
    public class Builder
    {
        private readonly List<CursorQuery> fromQuery = new();

        /// <summary>
        /// Gets the list of <see cref="Queries.CursorQuery"/>.
        /// </summary>
        public IReadOnlyList<CursorQuery> CursorQuery { get; }

        /// <summary>
        /// Gets or sets <c>true</c> whether the position is on the given values, relative to the sort order defined by the query; otherwise, <c>false</c> to skip the given values.
        /// </summary>
        public bool OnGiven { get; set; } = true;

        internal Builder()
        {
            CursorQuery = fromQuery.AsReadOnly();
        }

        /// <summary>
        /// Sets the cursor to specify that the position is on the given values, relative to the sort order defined by the query. This is the default configuration.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/>
        /// </returns>
        public Builder OnGivenValues()
        {
            OnGiven = true;
            return this;
        }

        /// <summary>
        /// Sets the cursor to specify that the position will skip the given values and select the next one, relative to the sort order defined by the query.
        /// </summary>
        /// <returns>
        /// The <see cref="Builder"/>
        /// </returns>
        public Builder OnNextValues()
        {
            OnGiven = false;
            return this;
        }

        /// <summary>
        /// Adds an instance of <see cref="Queries.CursorQuery"/> to the builder.
        /// </summary>
        /// <param name="value">
        /// The value that represent a position, in the order they appear in the order by clause of a query. Can contain fewer values than specified in the order by clause.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is a null reference.
        /// </exception>
        public Builder Add(object? value)
        {
            fromQuery.Add(new(value));
            return this;
        }

        /// <summary>
        /// Adds an instance of <see cref="Queries.CursorQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.CursorQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder Add(CursorQuery orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            fromQuery.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="Queries.CursorQuery"/> to the builder.
        /// </summary>
        /// <param name="orderBy">
        /// The multiple of <see cref="Queries.CursorQuery"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<CursorQuery> orderBy)
        {
            ArgumentNullException.ThrowIfNull(orderBy);

            fromQuery.AddRange(orderBy);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="Queries.CursorQuery"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.CursorQuery"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(CursorQuery orderBy)
        {
            return new Builder().Add(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.CursorQuery"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.CursorQuery"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(CursorQuery[] orderBy)
        {
            return new Builder().AddRange(orderBy);
        }

        /// <summary>
        /// Converts the <see cref="Queries.CursorQuery"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="orderBy">
        /// The <see cref="Queries.CursorQuery"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="orderBy"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<CursorQuery> orderBy)
        {
            return new Builder().AddRange(orderBy);
        }
    }

    /// <inheritdoc cref="Builder.OnNextValues()"/>
    public static Builder OnNextValues()
    {
        return new Builder().OnNextValues();
    }

    /// <inheritdoc cref="Builder.OnGivenValues()"/>
    public static Builder OnGivenValues()
    {
        return new Builder().OnGivenValues();
    }

    /// <inheritdoc cref="Builder.Add(object?)"/>
    public static Builder Add(object? value)
    {
        return new Builder().Add(value);
    }

    /// <inheritdoc cref="Builder.Add(CursorQuery)"/>
    public static Builder Add(CursorQuery orderBy)
    {
        return new Builder().Add(orderBy);
    }

    /// <inheritdoc cref="Builder.AddRange(IEnumerable{CursorQuery})"/>
    public static Builder AddRange(IEnumerable<CursorQuery> orderBy)
    {
        return new Builder().AddRange(orderBy);
    }

    /// <summary>
    /// Gets or sets the values that represent a position, in the order they appear in the order by clause of a query. Can contain fewer values than specified in the order by clause.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Creates an instance of <see cref="CursorQuery"/>.
    /// </summary>
    /// <param name="value">
    /// The value that represent a position, in the order they appear in the order by clause of a query. Can contain fewer values than specified in the order by clause.
    /// </param>
    public CursorQuery(object? value)
    {
        Value = value;
    }
}
