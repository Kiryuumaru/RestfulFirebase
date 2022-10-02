using System.Runtime.Serialization;

namespace RestfulFirebase.FirestoreDatabase.Enums;

/// <summary>
/// The operator for <see cref="Queries.UnaryFilterQuery"/>.
/// </summary>
public enum UnaryOperator
{
    /// <summary>
    /// The given field is equal to NaN.
    /// </summary>
    [EnumMember(Value = "IS_NAN")]
    IsNan,

    /// <summary>
    /// The given field is equal to NULL.
    /// </summary>
    [EnumMember(Value = "IS_NULL")]
    IsNull,

    /// <summary>
    /// The given field is not equal to NaN.
    /// <para>Requires:</para>
    /// <para>* No other <see cref="FieldOperator.NotEqual"/>, <see cref="FieldOperator.NotIn"/>, <see cref="IsNotNan"/>, or <see cref="IsNotNull"/>.</para>
    /// <para>* That field comes first in the orderBy.</para>
    /// </summary>
    [EnumMember(Value = "IS_NOT_NAN")]
    IsNotNan,

    /// <summary>
    /// The given field is not equal to NaN.
    /// <para>Requires:</para>
    /// <para>* A single <see cref="FieldOperator.NotEqual"/>, <see cref="FieldOperator.NotIn"/>, <see cref="IsNotNan"/>, or <see cref="IsNotNull"/>.</para>
    /// <para>* That field comes first in the orderBy.</para>
    /// </summary>
    [EnumMember(Value = "IS_NOT_NULL")]
    IsNotNull
}
