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
    /// <param name="namePath">
    /// The property name to which to apply the operator.
    /// </param>
    /// <param name="operator">
    /// The <see cref="UnaryOperator"/> to apply.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="namePath"/> is a null reference.
    /// </exception>
    public UnaryFilterQuery(string[] namePath, UnaryOperator @operator)
        : base(namePath)
    {
        ArgumentNullException.ThrowIfNull(namePath);

        Operator = @operator;
    }
}
