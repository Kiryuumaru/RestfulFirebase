using System.Runtime.Serialization;

namespace RestfulFirebase.FirestoreDatabase.Enums;

/// <summary>
/// The operator for <see cref="Queries.FieldFilterQuery"/>.
/// </summary>
public enum FieldOperator
{
    /// <summary>
    /// The given field is less than the given value.
    /// <para>Requires:</para>
    /// <para>* That field come first in orderBy.</para>
    /// </summary>
    [EnumMember(Value = "LESS_THAN")]
    LessThan,

    /// <summary>
    /// The given field is less than or equal to the given value.
    /// <para>Requires:</para>
    /// <para>* That field come first in orderBy.</para>
    /// </summary>
    [EnumMember(Value = "LESS_THAN_OR_EQUAL")]
    LessThanOrEqual,

    /// <summary>
    /// The given field is greater than the given value.
    /// <para>Requires:</para>
    /// <para>* That field come first in orderBy.</para>
    /// </summary>
    [EnumMember(Value = "GREATER_THAN")]
    GreaterThan,

    /// <summary>
    /// The given field is greater than or equal to the given value.
    /// <para>Requires:</para>
    /// <para>* That field come first in orderBy.</para>
    /// </summary>
    [EnumMember(Value = "GREATER_THAN_OR_EQUAL")]
    GreaterThanOrEqual,

    /// <summary>
    /// The given field is equal to the given value.
    /// </summary>
    [EnumMember(Value = "EQUAL")]
    Equal,

    /// <summary>
    /// The given field is not equal to the given value.
    /// <para>Requires:</para>
    /// <para>* No other <see cref="NotEqual"/>, <see cref="NotIn"/>, <see cref="UnaryOperator.IsNotNull"/>, or <see cref="UnaryOperator.IsNotNan"/>.</para>
    /// <para>* That field comes first in the orderBy.</para>
    /// </summary>
    [EnumMember(Value = "NOT_EQUAL")]
    NotEqual,

    /// <summary>
    /// The given field is an array that contains the given value.
    /// </summary>
    [EnumMember(Value = "ARRAY_CONTAINS")]
    ArrayContains,

    /// <summary>
    /// The given field is equal to at least one value in the given array.
    /// <para>Requires:</para>
    /// <para>* That value is a non-empty ArrayValue with at most 10 values.</para>
    /// <para>* No other <see cref="In"/>, <see cref="ArrayContainsAny"/> or <see cref="NotIn"/>.</para>
    /// </summary>
    [EnumMember(Value = "IN")]
    In,

    /// <summary>
    /// The given field is an array that contains any of the values in the given array.
    /// <para>Requires:</para>
    /// <para>* That value is a non-empty ArrayValue with at most 10 values.</para>
    /// <para>* No other <see cref="In"/>, <see cref="ArrayContainsAny"/> or <see cref="NotIn"/>.</para>
    /// </summary>
    [EnumMember(Value = "ARRAY_CONTAINS_ANY")]
    ArrayContainsAny,

    /// <summary>
    /// The value of the field is not in the given array.
    /// <para>Requires:</para>
    /// <para>* That value is a non-empty ArrayValue with at most 10 values.</para>
    /// <para>* No other <see cref="In"/>, <see cref="ArrayContainsAny"/>, <see cref="NotIn"/>, <see cref="NotEqual"/>, <see cref="UnaryOperator.IsNotNull"/>, or <see cref="UnaryOperator.IsNotNan"/>.</para>
    /// <para>* That field comes first in the orderBy.</para>
    /// </summary>
    [EnumMember(Value = "NOT_IN")]
    NotIn,
}
