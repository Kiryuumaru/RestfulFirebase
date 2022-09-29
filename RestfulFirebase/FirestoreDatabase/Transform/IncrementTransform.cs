using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "increment" transformation parameter for "increment" transform commit writes.
/// </summary>
public class IncrementTransform : FieldTransform
{
    /// <summary>
    /// Creates field "increment" transformation parameter for "increment" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "increment".
    /// </typeparam>
    /// <param name="incrementValue">
    /// The value to "increment" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "increment".
    /// </param>
    /// <returns>
    /// The created <see cref="IncrementTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="incrementValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static IncrementTransform Create<TModel>(object incrementValue, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(incrementValue);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(incrementValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "increment" transformation parameter for "increment" transform commit writes.
    /// </summary>
    /// <param name="incrementValue">
    /// The value to "increment" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "increment".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "increment".
    /// </param>
    /// <returns>
    /// The created <see cref="IncrementTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="incrementValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static IncrementTransform Create(object incrementValue, Type modelType, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(incrementValue);
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        return new(incrementValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "increment" to the given property path.
    /// </summary>
    public object IncrementValue { get; }

    internal IncrementTransform(object incrementValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        IncrementValue = incrementValue;
    }
}
