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
/// The "where" parameter for query with a single operand.
/// </summary>
public class UnaryFilterQuery : FilterQuery
{
    /// <summary>
    /// Creates new instance of <see cref="UnaryFilterQuery"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <returns>
    /// The created <see cref="UnaryFilterQuery"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public static UnaryFilterQuery Create(string propertyName, UnaryOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        return new(propertyName, @operator);
    }

    /// <summary>
    /// Gets or sets the <see cref="UnaryOperator"/> to apply.
    /// </summary>
    public UnaryOperator Operator { get; set; }

    internal UnaryFilterQuery(string propertyName, UnaryOperator @operator)
        : base(propertyName)
    {
        Operator = @operator;
    }
}
