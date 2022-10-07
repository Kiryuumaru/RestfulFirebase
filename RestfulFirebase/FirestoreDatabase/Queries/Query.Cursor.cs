using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class BaseQuery<TQuery>
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
        if (startCursorQuery.Count > 0 &&
            IsStartAfter)
        {
            throw new ArgumentException($"Cannot combine \"{nameof(StartAt)}\" with \"{nameof(StartAfter)}\" query.");
        }

        IsStartAfter = false;
        startCursorQuery.Add(new(value));

        return (TQuery)this;
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
        if (startCursorQuery.Count > 0 &&
            !IsStartAfter)
        {
            throw new ArgumentException($"Cannot combine \"{nameof(StartAt)}\" with \"{nameof(StartAfter)}\" query.");
        }

        startCursorQuery.Add(new(value));
        IsStartAfter = true;

        return (TQuery)this;
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
        if (endCursorQuery.Count > 0 &&
            IsEndBefore)
        {
            throw new ArgumentException($"Cannot combine \"{nameof(EndAt)}\" with \"{nameof(EndAfter)}\" query.");
        }

        IsEndBefore = false;
        endCursorQuery.Add(new(value));

        return (TQuery)this;
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
    public TQuery EndAfter(object? value)
    {
        if (endCursorQuery.Count > 0 &&
            !IsEndBefore)
        {
            throw new ArgumentException($"Cannot combine \"{nameof(EndAt)}\" with \"{nameof(EndAfter)}\" query.");
        }

        endCursorQuery.Add(new(value));
        IsEndBefore = true;

        return (TQuery)this;
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
