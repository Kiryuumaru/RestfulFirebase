﻿using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field transformation parameter for transform commit writes.
/// </summary>
public abstract class FieldTransform
{
    /// <summary>
    /// The builder for <see cref="FieldTransform"/>.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Creates an instance of <see cref="Builder"/>.
        /// </summary>
        /// <returns>
        /// The created <see cref="Builder"/>.
        /// </returns>
        public static Builder Create()
        {
            return new();
        }

        /// <summary>
        /// Gets the list of built <see cref="FieldTransform"/>.
        /// </summary>
        public List<FieldTransform> FieldTransforms { get; } = new();

        /// <summary>
        /// Adds field increment transformation parameter for increment transform commit writes.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to increment.
        /// </typeparam>
        /// <param name="incrementValue">
        /// The value to increment to the model <typeparamref name="TModel"/>.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to increment.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="incrementValue"/> and
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Increment<TModel>(object incrementValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(new FieldTransformIncrement(incrementValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds field increment transformation parameter for increment transform commit writes.
        /// </summary>
        /// <param name="incrementValue">
        /// The value to increment to the model <paramref name="modelType"/>.
        /// </param>
        /// <param name="modelType">
        /// The type of the model to increment.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to increment.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="incrementValue"/>,
        /// <paramref name="modelType"/> and
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Increment(object incrementValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(new FieldTransformIncrement(incrementValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds field maximum transformation parameter for maximum transform commit writes.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to maximum.
        /// </typeparam>
        /// <param name="maximumValue">
        /// The value to maximum to the model <typeparamref name="TModel"/>.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to maximum.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="maximumValue"/> and
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Maximum<TModel>(object maximumValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(new FieldTransformMaximum(maximumValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds field maximum transformation parameter for maximum transform commit writes.
        /// </summary>
        /// <param name="maximumValue">
        /// The value to maximum to the model <paramref name="modelType"/>.
        /// </param>
        /// <param name="modelType">
        /// The type of the model to maximum.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to maximum.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="maximumValue"/>,
        /// <paramref name="modelType"/> and
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Maximum(object maximumValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(new FieldTransformMaximum(maximumValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds field minimum transformation parameter for minimum transform commit writes.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to minimum.
        /// </typeparam>
        /// <param name="minimumValue">
        /// The value to minimum to the model <typeparamref name="TModel"/>.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to minimum.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="minimumValue"/> and
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Minimum<TModel>(object minimumValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(new FieldTransformMinimum(minimumValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds field minimum transformation parameter for minimum transform commit writes.
        /// </summary>
        /// <param name="minimumValue">
        /// The value to minimum to the model <paramref name="modelType"/>.
        /// </param>
        /// <param name="modelType">
        /// The type of the model to minimum.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to minimum.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="minimumValue"/>,
        /// <paramref name="modelType"/> and
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Minimum(object minimumValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(new FieldTransformMinimum(minimumValue, modelType, propertyNamePath));
            return this;
        }
    }

    /// <summary>
    /// Gets the type of the model to transform.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    /// Gets the path of the property.
    /// </summary>
    public string[] PropertyNamePath { get; }

    internal FieldTransform(Type modelType, string[] propertyNamePath)
    {
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(propertyNamePath);

        ModelType = modelType;
        PropertyNamePath = propertyNamePath;
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    internal abstract void BuildUtf8JsonWriter(Utf8JsonWriter writer, FirebaseConfig config, JsonSerializerOptions? jsonSerializerOptions);

    internal static NumberType GetNumberType(Type incrementType)
    {
        if (incrementType.IsAssignableFrom(typeof(sbyte)) ||
            incrementType.IsAssignableFrom(typeof(byte)) ||
            incrementType.IsAssignableFrom(typeof(short)) ||
            incrementType.IsAssignableFrom(typeof(ushort)) ||
            incrementType.IsAssignableFrom(typeof(int)) ||
            incrementType.IsAssignableFrom(typeof(uint)) ||
            incrementType.IsAssignableFrom(typeof(long)) ||
            incrementType.IsAssignableFrom(typeof(ulong)) ||
            incrementType.IsAssignableFrom(typeof(nint)) ||
            incrementType.IsAssignableFrom(typeof(nuint)))
        {
            return NumberType.Integer;
        }
        else if (
            incrementType.IsAssignableFrom(typeof(float)) ||
            incrementType.IsAssignableFrom(typeof(double)))
        {
            return NumberType.Double;
        }
        else if (
            incrementType.IsAssignableFrom(typeof(decimal)))
        {
            throw new ArgumentException("Decimal number is not yet supported.");
        }
        else
        {
            throw new ArgumentException($"\"{incrementType}\" type is not supported.");
        }
    }
}
