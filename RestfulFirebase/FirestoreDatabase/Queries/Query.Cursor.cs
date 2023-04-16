using System;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class FluentQuery<TQuery>
{
    /// <summary>
    /// Adds an instance of <see cref="CursorQuery"/> with the start at configuration to the query.
    /// </summary>
    /// <param name="value">
    /// The values that represent a position, in the order they appear in the order by clause of a query. Can contain fewer values than specified in the order by clause.
    /// </param>
    /// <returns>
    /// The query with new added start <see cref="CursorQuery"/>.
    /// </returns>
    public TQuery StartAt(object? value)
    {
        TQuery query = (TQuery)Clone();

        query.IsStartAfter = false;
        query.WritableStartCursorQuery.Add(new(value));

        return query;
    }

    /// <summary>
    /// Adds an instance of <see cref="CursorQuery"/> with the start after configuration to the query.
    /// </summary>
    /// <param name="value">
    /// The values that represent a position, in the order they appear in the order by clause of a query. Can contain fewer values than specified in the order by clause.
    /// </param>
    /// <returns>
    /// The query with new added start <see cref="CursorQuery"/>.
    /// </returns>
    public TQuery StartAfter(object? value)
    {
        TQuery query = (TQuery)Clone();

        query.IsStartAfter = true;
        query.WritableStartCursorQuery.Add(new(value));

        return query;
    }

    /// <summary>
    /// Adds an instance of <see cref="CursorQuery"/> with the end at configuration to the query.
    /// </summary>
    /// <param name="value">
    /// The value that represent a position, in the order they appear in the order by clause of a query.
    /// </param>
    /// <returns>
    /// The query with new added end <see cref="CursorQuery"/>.
    /// </returns>
    public TQuery EndAt(object? value)
    {
        TQuery query = (TQuery)Clone();

        query.IsEndBefore = false;
        query.WritableEndCursorQuery.Add(new(value));

        return query;
    }

    /// <summary>
    /// Adds an instance of <see cref="CursorQuery"/> with the end before configuration to the query.
    /// </summary>
    /// <param name="value">
    /// The value that represent a position, in the order they appear in the order by clause of a query.
    /// </param>
    /// <returns>
    /// The query with new added end <see cref="CursorQuery"/>.
    /// </returns>
    public TQuery EndBefore(object? value)
    {
        TQuery query = (TQuery)Clone();

        query.IsEndBefore = true;
        query.WritableEndCursorQuery.Add(new(value));

        return query;
    }
}

/// <summary>
/// The "startAt" or "endAt" parameter for query.
/// </summary>
public class CursorQuery
{
    /// <summary>
    /// Gets the values that represent a position, in the order they appear in the order by clause of a query. Can contain fewer values than specified in the order by clause.
    /// </summary>
    public object? Value { get; internal set; }

    internal CursorQuery(object? value)
    {
        Value = value;
    }
}

internal class StructuredCursor
{
    public CursorQuery CursorQuery { get; internal set; }

    public Type? ValueType { get; internal set; }

    public object? Value { get; internal set; }

    internal StructuredCursor(CursorQuery cursorQuery, Type? valueType, object? value)
    {
        CursorQuery = cursorQuery;
        ValueType = valueType;
        Value = value;
    }
}
