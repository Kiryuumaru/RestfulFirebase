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
/// The field "minimum" transformation parameter for "minimum" transform commit writes.
/// </summary>
public class MinimumTransform : FieldTransform
{
    /// <summary>
    /// Creates field "minimum" transformation parameter for "minimum" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "minimum".
    /// </typeparam>
    /// <param name="minimumValue">
    /// The value to "minimum" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "minimum".
    /// </param>
    /// <returns>
    /// The created <see cref="MinimumTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minimumValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static MinimumTransform Create<TModel>(object minimumValue, string[] propertyNamePath)
    {
        return new(minimumValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "minimum" transformation parameter for "minimum" transform commit writes.
    /// </summary>
    /// <param name="minimumValue">
    /// The value to "minimum" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "minimum".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "minimum".
    /// </param>
    /// <returns>
    /// The created <see cref="MinimumTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="minimumValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static MinimumTransform Create(object minimumValue, Type modelType, string[] propertyNamePath)
    {
        return new(minimumValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "minimum" to the given property path.
    /// </summary>
    public object MinimumValue { get; }

    internal MinimumTransform(object minimumValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(minimumValue);

        MinimumValue = minimumValue;
    }
}
