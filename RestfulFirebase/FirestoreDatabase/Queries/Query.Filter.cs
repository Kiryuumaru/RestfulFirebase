using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class FluentQueryRoot<TQuery>
{
    #region Main

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <param name="documentFieldPath">
    /// The document field path to which to apply the operator.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public TQuery Where(UnaryOperator @operator, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);
        ArgumentException.ThrowIfHasNullOrEmpty(documentFieldPath);

        TQuery query = (TQuery)Clone();

        query.WritableWhereQuery.Add(new UnaryFilterQuery(documentFieldPath, false, @operator));

        return query;
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
    /// <param name="documentFieldPath">
    /// The document field path to which to apply the operator.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="documentFieldPath"/> is empty.
    /// </exception>
    public TQuery Where(FieldOperator @operator, object? value, params string[] documentFieldPath)
    {
        ArgumentNullException.ThrowIfNull(documentFieldPath);
        ArgumentException.ThrowIfHasNullOrEmpty(documentFieldPath);

        TQuery query = (TQuery)Clone();

        query.WritableWhereQuery.Add(new FieldFilterQuery(documentFieldPath, false, @operator, value));

        return query;
    }

    #endregion

    #region Additionals

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldName">
    /// The document field name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldName"/> is a null reference.
    /// </exception>
    public TQuery Where(string documentFieldName, UnaryOperator @operator) => Where(@operator, documentFieldName);

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldName">
    /// The document field name to which to apply the operator.
    /// </param>
    /// <param name="subDocumentFieldName">
    /// The sub document field name of <paramref name="subDocumentFieldName"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldName"/> or
    /// <paramref name="subDocumentFieldName"/> is a null reference.
    /// </exception>
    public TQuery Where(string documentFieldName, string subDocumentFieldName, UnaryOperator @operator) => Where(@operator, documentFieldName, subDocumentFieldName);

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldName">
    /// The document field name to which to apply the operator.
    /// </param>
    /// <param name="subDocumentFieldName1">
    /// The sub document field name of <paramref name="documentFieldName"/> to which to apply the operator.
    /// </param>
    /// <param name="subDocumentFieldName2">
    /// The sub document field name of <paramref name="subDocumentFieldName1"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentFieldName"/>,
    /// <paramref name="subDocumentFieldName1"/> or
    /// <paramref name="subDocumentFieldName2"/> is a null reference.
    /// </exception>
    public TQuery Where(string documentFieldName, string subDocumentFieldName1, string subDocumentFieldName2, UnaryOperator @operator) => Where(@operator, documentFieldName, subDocumentFieldName1, subDocumentFieldName2);

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldName">
    /// The document field name to which to apply the operator.
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
    /// <paramref name="documentFieldName"/> is a null reference.
    /// </exception>
    public TQuery Where(string documentFieldName, FieldOperator @operator, object? value) => Where(@operator, value, documentFieldName);

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldName">
    /// The document field name to which to apply the operator.
    /// </param>
    /// <param name="subDocumentFieldName">
    /// The sub document field name of <paramref name="documentFieldName"/> to which to apply the operator.
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
    /// <paramref name="documentFieldName"/> or
    /// <paramref name="subDocumentFieldName"/> is a null reference.
    /// </exception>
    public TQuery Where(string documentFieldName, string subDocumentFieldName, FieldOperator @operator, object? value) => Where(@operator, value, documentFieldName, subDocumentFieldName);

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="documentFieldName">
    /// The document field name to which to apply the operator.
    /// </param>
    /// <param name="subDocumentFieldName1">
    /// The sub document field name of <paramref name="documentFieldName"/> to which to apply the operator.
    /// </param>
    /// <param name="subDocumentFieldName2">
    /// The sub document field name of <paramref name="subDocumentFieldName1"/> to which to apply the operator.
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
    /// <paramref name="documentFieldName"/>,
    /// <paramref name="subDocumentFieldName1"/> or
    /// <paramref name="subDocumentFieldName2"/> is a null reference.
    /// </exception>
    public TQuery Where(string documentFieldName, string subDocumentFieldName1, string subDocumentFieldName2, FieldOperator @operator, object? value) => Where(@operator, value, documentFieldName, subDocumentFieldName1, subDocumentFieldName2);

    #endregion
}

public partial class FluentQueryRoot<TQuery, TModel>
{
    #region Main

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <param name="propertyPath">
    /// The property name to which to apply the operator.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public TQuery PropertyWhere(UnaryOperator @operator, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);
        ArgumentException.ThrowIfHasNullOrEmpty(propertyPath);

        TQuery query = (TQuery)Clone();

        query.WritableWhereQuery.Add(new UnaryFilterQuery(propertyPath, true, @operator));

        return query;
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
    /// <param name="propertyPath">
    /// The property path to which to apply the operator.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyPath"/> is a null reference.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="propertyPath"/> is empty.
    /// </exception>
    public TQuery PropertyWhere(FieldOperator @operator, object? value, params string[] propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);
        ArgumentException.ThrowIfHasNullOrEmpty(propertyPath);

        TQuery query = (TQuery)Clone();

        query.WritableWhereQuery.Add(new FieldFilterQuery(propertyPath, true, @operator, value));

        return query;
    }

    #endregion

    #region Additionals

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public TQuery PropertyWhere(string propertyName, UnaryOperator @operator)
        => PropertyWhere(@operator, propertyName);

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subPropertyName">
    /// The sub property name of <paramref name="propertyName"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> or
    /// <paramref name="subPropertyName"/> is a null reference.
    /// </exception>
    public TQuery PropertyWhere(string propertyName, string subPropertyName, UnaryOperator @operator)
        => PropertyWhere(@operator, propertyName, subPropertyName);

    /// <summary>
    /// Adds new instance of <see cref="UnaryFilterQuery"/> to the query.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subPropertyName1">
    /// The sub property name of <paramref name="propertyName"/> to which to apply the operator.
    /// </param>
    /// <param name="subPropertyName2">
    /// The sub property name of <paramref name="subPropertyName1"/> to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The query with new added "where" query.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/>,
    /// <paramref name="subPropertyName1"/> or
    /// <paramref name="subPropertyName2"/> is a null reference.
    /// </exception>
    public TQuery PropertyWhere(string propertyName, string subPropertyName1, string subPropertyName2, UnaryOperator @operator)
        => PropertyWhere(@operator, propertyName, subPropertyName1, subPropertyName2);

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="propertyName">
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
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public TQuery PropertyWhere(string propertyName, FieldOperator @operator, object? value)
        => PropertyWhere(@operator, value, propertyName);

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subPropertyName">
    /// The sub property name of <paramref name="propertyName"/> to which to apply the operator.
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
    /// <paramref name="propertyName"/> or
    /// <paramref name="subPropertyName"/> is a null reference.
    /// </exception>
    public TQuery PropertyWhere(string propertyName, string subPropertyName, FieldOperator @operator, object? value)
        => PropertyWhere(@operator, value, propertyName, subPropertyName);

    /// <summary>
    /// Adds new instance of <see cref="FieldFilterQuery"/> to the query.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="subPropertyName1">
    /// The sub property name of <paramref name="propertyName"/> to which to apply the operator.
    /// </param>
    /// <param name="subPropertyName2">
    /// The sub property name of <paramref name="subPropertyName1"/> to which to apply the operator.
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
    /// <paramref name="propertyName"/>,
    /// <paramref name="subPropertyName1"/> or
    /// <paramref name="subPropertyName2"/> is a null reference.
    /// </exception>
    public TQuery PropertyWhere(string propertyName, string subPropertyName1, string subPropertyName2, FieldOperator @operator, object? value)
        => PropertyWhere(@operator, value, propertyName, subPropertyName1, subPropertyName2);

    #endregion
}

/// <summary>
/// The "where" parameter for query.
/// </summary>
public abstract class FilterQuery
{
    /// <summary>
    /// Gets the path of the property or document field to filter.
    /// </summary>
    public string[] NamePath { get; internal set; }

    /// <summary>
    /// Gets <c>true</c> if the <see cref="NamePath"/> is a property name; otherwise <c>false</c> if it is a document field name.
    /// </summary>
    public bool IsNamePathAPropertyPath { get; internal set; }

    internal FilterQuery(string[] namePath, bool isPathPropertyName)
    {
        NamePath = namePath;
        IsNamePathAPropertyPath = isPathPropertyName;
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

    internal UnaryFilterQuery(string[] namePath, bool isPathPropertyName, UnaryOperator @operator)
        : base(namePath, isPathPropertyName)
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

    internal FieldFilterQuery(string[] namePath, bool isPathPropertyName, FieldOperator @operator, object? value)
        : base(namePath, isPathPropertyName)
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
