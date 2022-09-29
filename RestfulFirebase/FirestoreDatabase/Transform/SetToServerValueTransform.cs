using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
/// </summary>
public class SetToServerValueTransform : FieldTransform
{
    /// <summary>
    /// Creates field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "setToServerValue".
    /// </typeparam>
    /// <param name="setToServerValue">
    /// The value to "setToServerValue" to the model <typeparamref name="TModel"/>.
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "setToServerValue".
    /// </param>
    /// <returns>
    /// The created <see cref="SetToServerValueTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="setToServerValue"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static SetToServerValueTransform Create<TModel>(ServerValue setToServerValue, string[] propertyNamePath)
    {
        return new(setToServerValue, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
    /// </summary>
    /// <param name="setToServerValue">
    /// The value to "setToServerValue" to the model <paramref name="modelType"/>.
    /// </param>
    /// <param name="modelType">
    /// The type of the model to "setToServerValue".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "setToServerValue".
    /// </param>
    /// <returns>
    /// The created <see cref="SetToServerValueTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="setToServerValue"/>,
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static SetToServerValueTransform Create(ServerValue setToServerValue, Type modelType, string[] propertyNamePath)
    {
        return new(setToServerValue, modelType, propertyNamePath);
    }

    /// <summary>
    /// Creates field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "setToServerValue".
    /// </typeparam>
    /// <param name="propertyNamePath">
    /// The property path of the model to "setToServerValue".
    /// </param>
    /// <returns>
    /// The created <see cref="SetToServerValueTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static SetToServerValueTransform RequestTime<TModel>(string[] propertyNamePath)
    {
        return new(ServerValue.RequestTime, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
    /// </summary>
    /// <param name="modelType">
    /// The type of the model to "setToServerValue".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "setToServerValue".
    /// </param>
    /// <returns>
    /// The created <see cref="SetToServerValueTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static SetToServerValueTransform RequestTime(Type modelType, string[] propertyNamePath)
    {
        return new(ServerValue.RequestTime, modelType, propertyNamePath);
    }

    /// <summary>
    /// Creates field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
    /// </summary>
    /// <typeparam name="TModel">
    /// The type of the model to "setToServerValue".
    /// </typeparam>
    /// <param name="propertyNamePath">
    /// The property path of the model to "setToServerValue".
    /// </param>
    /// <returns>
    /// The created <see cref="SetToServerValueTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static SetToServerValueTransform ServerValueUnspecified<TModel>(string[] propertyNamePath)
    {
        return new(ServerValue.ServerValueUnspecified, typeof(TModel), propertyNamePath);
    }

    /// <summary>
    /// Creates field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
    /// </summary>
    /// <param name="modelType">
    /// The type of the model to "setToServerValue".
    /// </param>
    /// <param name="propertyNamePath">
    /// The property path of the model to "setToServerValue".
    /// </param>
    /// <returns>
    /// The created <see cref="SetToServerValueTransform"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="modelType"/> and
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public static SetToServerValueTransform ServerValueUnspecified(Type modelType, string[] propertyNamePath)
    {
        return new(ServerValue.ServerValueUnspecified, modelType, propertyNamePath);
    }

    /// <summary>
    /// Gets the object to "setToServerValue" to the given property path.
    /// </summary>
    public ServerValue SetToServerValue { get; }

    internal SetToServerValueTransform(ServerValue setToServerValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        SetToServerValue = setToServerValue;
    }
}
