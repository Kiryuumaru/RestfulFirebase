using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class BaseQuery<TQuery>
{
    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, UnaryOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(name);

        whereQuery.Add(new UnaryFilterQuery(new string[] { name }, @operator));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName, UnaryOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName);

        whereQuery.Add(new UnaryFilterQuery(new string[] { name, subName }, @operator));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName1">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="subName2">
    /// The sub property name of <paramref name="subName1"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName1"/> or
    /// <paramref name="subName2"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName1, string subName2, UnaryOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName1);
        ArgumentNullException.ThrowIfNull(subName2);

        whereQuery.Add(new UnaryFilterQuery(new string[] { name, subName1, subName2 }, @operator));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName1">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="subName2">
    /// The sub property name of <paramref name="subName1"/> to which to apply the operator.
    /// </param>
    /// <param name="subName3">
    /// The sub property name of <paramref name="subName2"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName1"/> or
    /// <paramref name="subName2"/> or
    /// <paramref name="subName3"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName1, string subName2, string subName3, UnaryOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName1);
        ArgumentNullException.ThrowIfNull(subName2);
        ArgumentNullException.ThrowIfNull(subName3);

        whereQuery.Add(new UnaryFilterQuery(new string[] { name, subName1, subName2, subName3 }, @operator));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName1">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="subName2">
    /// The sub property name of <paramref name="subName1"/> to which to apply the operator.
    /// </param>
    /// <param name="subName3">
    /// The sub property name of <paramref name="subName2"/> to which to apply the operator.
    /// </param>
    /// <param name="subName4">
    /// The sub property name of <paramref name="subName3"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName1"/> or
    /// <paramref name="subName2"/> or
    /// <paramref name="subName3"/> or
    /// <paramref name="subName4"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName1, string subName2, string subName3, string subName4, UnaryOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName1);
        ArgumentNullException.ThrowIfNull(subName2);
        ArgumentNullException.ThrowIfNull(subName3);
        ArgumentNullException.ThrowIfNull(subName4);

        whereQuery.Add(new UnaryFilterQuery(new string[] { name, subName1, subName2, subName3, subName4 }, @operator));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <param name="namePath">
    /// The property name to which to apply the operator.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="namePath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="namePath"/> is empty.
    /// </exception>
    public TQuery Where(UnaryOperator @operator, params string[] namePath)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        if (namePath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(namePath)}\" is empty.");
        }

        whereQuery.Add(new UnaryFilterQuery(namePath, @operator));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="FieldOperator"/> to apply.
    /// </param>
    /// <param name="value">
    /// The value to compare to.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, FieldOperator @operator, object? value)
    {
        ArgumentNullException.ThrowIfNull(name);

        whereQuery.Add(new FieldFilterQuery(new string[] { name }, @operator, value));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="FieldOperator"/> to apply.
    /// </param>
    /// <param name="value">
    /// The value to compare to.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName, FieldOperator @operator, object? value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName);

        whereQuery.Add(new FieldFilterQuery(new string[] { name, subName }, @operator, value));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName1">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="subName2">
    /// The sub property name of <paramref name="subName1"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="FieldOperator"/> to apply.
    /// </param>
    /// <param name="value">
    /// The value to compare to.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName1"/> or
    /// <paramref name="subName2"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName1, string subName2, FieldOperator @operator, object? value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName1);
        ArgumentNullException.ThrowIfNull(subName2);

        whereQuery.Add(new FieldFilterQuery(new string[] { name, subName1, subName2 }, @operator, value));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName1">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="subName2">
    /// The sub property name of <paramref name="subName1"/> to which to apply the operator.
    /// </param>
    /// <param name="subName3">
    /// The sub property name of <paramref name="subName2"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="FieldOperator"/> to apply.
    /// </param>
    /// <param name="value">
    /// The value to compare to.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName1"/> or
    /// <paramref name="subName2"/> or
    /// <paramref name="subName3"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName1, string subName2, string subName3, FieldOperator @operator, object? value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName1);
        ArgumentNullException.ThrowIfNull(subName2);
        ArgumentNullException.ThrowIfNull(subName3);

        whereQuery.Add(new FieldFilterQuery(new string[] { name, subName1, subName2, subName3 }, @operator, value));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="name">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subName1">
    /// The sub property name of <paramref name="name"/> to which to apply the operator.
    /// </param>
    /// <param name="subName2">
    /// The sub property name of <paramref name="subName1"/> to which to apply the operator.
    /// </param>
    /// <param name="subName3">
    /// The sub property name of <paramref name="subName2"/> to which to apply the operator.
    /// </param>
    /// <param name="subName4">
    /// The sub property name of <paramref name="subName3"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="FieldOperator"/> to apply.
    /// </param>
    /// <param name="value">
    /// The value to compare to.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or
    /// <paramref name="subName1"/> or
    /// <paramref name="subName2"/> or
    /// <paramref name="subName3"/> or
    /// <paramref name="subName4"/> is a null reference.
    /// </exception>
    public TQuery Where(string name, string subName1, string subName2, string subName3, string subName4, FieldOperator @operator, object? value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(subName1);
        ArgumentNullException.ThrowIfNull(subName2);
        ArgumentNullException.ThrowIfNull(subName3);
        ArgumentNullException.ThrowIfNull(subName4);

        whereQuery.Add(new FieldFilterQuery(new string[] { name, subName1, subName2, subName3, subName4 }, @operator, value));

        return (TQuery)this;
    }

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="operator">
    /// The <see cref="FieldOperator"/> to apply.
    /// </param>
    /// <param name="value">
    /// The value to compare to.
    /// </param>
    /// <param name="namePath">
    /// The property name to which to apply the operator.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="namePath"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="namePath"/> is empty.
    /// </exception>
    public TQuery Where(FieldOperator @operator, object? value, params string[] namePath)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        if (namePath.Length == 0)
        {
            throw new ArgumentException($"\"{nameof(namePath)}\" is empty.");
        }

        whereQuery.Add(new FieldFilterQuery(namePath, @operator, value));

        return (TQuery)this;
    }
}

/// <summary>
/// The "where" parameter for query.
/// </summary>
public abstract class FilterQuery
{
    /// <summary>
    /// Gets the path of the document field to filter.
    /// </summary>
    public string[] NamePath { get; internal set; }

    internal FilterQuery(string[] namePath)
    {
        NamePath = namePath;
    }
}

/// <summary>
/// The "where" parameter for query with a single operand.
/// </summary>
public class UnaryFilterQuery : FilterQuery
{
    /// <summary>
    /// Gets or sets the <see cref="UnaryOperator"/> to apply.
    /// </summary>
    public UnaryOperator Operator { get; set; }

    internal UnaryFilterQuery(string[] namePath, UnaryOperator @operator)
        : base(namePath)
    {
        Operator = @operator;
    }
}

/// <summary>
/// The "where" parameter for query on a specific field.
/// </summary>
public class FieldFilterQuery : FilterQuery
{
    /// <summary>
    /// Gets or sets the <see cref="FieldOperator"/> to apply.
    /// </summary>
    public FieldOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the value to compare to.
    /// </summary>
    public object? Value { get; set; }

    internal FieldFilterQuery(string[] namePath, FieldOperator @operator, object? value)
        : base(namePath)
    {
        Operator = @operator;
        Value = value;
    }
}

internal class StructuredFilter
{
    public FilterQuery FilterQuery { get; internal set; }

    public string DocumentFieldPath { get; internal set; }

    internal StructuredFilter(FilterQuery filterQuery, string documentFieldPath)
    {
        FilterQuery = filterQuery;
        DocumentFieldPath = documentFieldPath;
    }
}
