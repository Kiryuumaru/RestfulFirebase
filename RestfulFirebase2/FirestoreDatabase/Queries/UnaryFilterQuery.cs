using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The "where" parameter for query with a single operand.
/// </summary>
public class UnaryFilterQuery : FilterQuery
{
    /// <summary>
    /// Gets or sets the <see cref="UnaryOperator"/> to apply.
    /// </summary>
    public UnaryOperator Operator { get; set; }

    /// <summary>
    /// Creates new instance of <see cref="UnaryFilterQuery"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is a null reference.
    /// </exception>
    public UnaryFilterQuery(string propertyName, UnaryOperator @operator)
        : base(propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        Operator = @operator;
    }
}
