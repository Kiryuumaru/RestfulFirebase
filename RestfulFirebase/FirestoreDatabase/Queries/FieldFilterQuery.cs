using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.FirestoreDatabase.Queries;

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

    /// <summary>
    /// Creates new instance of <see cref="FieldFilterQuery"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <param name="value">
    /// The value to compare to.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public FieldFilterQuery(string propertyName, FieldOperator @operator, object? value)
        : base(propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        Operator = @operator;
        Value = value;
    }
}
