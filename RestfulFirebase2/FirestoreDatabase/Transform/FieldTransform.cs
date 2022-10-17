using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Transforms;

/// <summary>
/// The field transformation parameter for transform commit writes to the <see cref="Builder"/>.
/// </summary>
public abstract class FieldTransform
{
    /// <summary>
    /// The builder for <see cref="FieldTransform"/>.
    /// </summary>
    public class Builder
    {
        private readonly List<FieldTransform> fieldTransforms = new();

        /// <summary>
        /// Gets the list of <see cref="FieldTransform"/>.
        /// </summary>
        public IReadOnlyList<FieldTransform> FieldTransforms { get; }

        internal Builder()
        {
            FieldTransforms = fieldTransforms.AsReadOnly();
        }

        /// <summary>
        /// Adds the <see cref="FieldTransform"/> to the <see cref="DocumentTransform.Builder"/>.
        /// </summary>
        /// <param name="documentReference">
        /// The <see cref="DocumentReference"/> to transform.
        /// </param>
        /// <returns>
        /// The <see cref="DocumentTransform.Builder"/> with new added field transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentReference"/> is a null reference.
        /// </exception>
        public DocumentTransform.Builder DocumentTransform(DocumentReference documentReference)
        {
            return new DocumentTransform.Builder().Add(documentReference, fieldTransforms);
        }

        /// <summary>
        /// Adds new field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to "appendMissingElements".
        /// </typeparam>
        /// <param name="appendMissingElementsValue">
        /// The value to "appendMissingElements" to the model <typeparamref name="TModel"/>.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "appendMissingElements".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appendMissingElementsValue"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder AppendMissingElements<TModel>(IEnumerable<object> appendMissingElementsValue, params string[] propertyNamePath)
            where TModel : class
        {
            fieldTransforms.Add(new AppendMissingElementsTransform(appendMissingElementsValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "appendMissingElements" transformation parameter for "appendMissingElements" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="appendMissingElementsValue">
        /// The value to "appendMissingElements" to the model <paramref name="modelType"/>.
        /// </param>
        /// <param name="modelType">
        /// The type of the model to "appendMissingElements".
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "appendMissingElements".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appendMissingElementsValue"/>,
        /// <paramref name="modelType"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder AppendMissingElements(IEnumerable<object> appendMissingElementsValue, Type modelType, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new AppendMissingElementsTransform(appendMissingElementsValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "increment" transformation parameter for "increment" transform commit writes to the <see cref="Builder"/>.
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
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="incrementValue"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Increment<TModel>(object incrementValue, params string[] propertyNamePath)
            where TModel : class
        {
            fieldTransforms.Add(new IncrementTransform(incrementValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "increment" transformation parameter for "increment" transform commit writes to the <see cref="Builder"/>.
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
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="incrementValue"/>,
        /// <paramref name="modelType"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Increment(object incrementValue, Type modelType, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new IncrementTransform(incrementValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "maximum" transformation parameter for "maximum" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to "maximum".
        /// </typeparam>
        /// <param name="maximumValue">
        /// The value to "maximum" to the model <typeparamref name="TModel"/>.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "maximum".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="maximumValue"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Maximum<TModel>(object maximumValue, params string[] propertyNamePath)
            where TModel : class
        {
            fieldTransforms.Add(new MaximumTransform(maximumValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "maximum" transformation parameter for "maximum" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="maximumValue">
        /// The value to "maximum" to the model <paramref name="modelType"/>.
        /// </param>
        /// <param name="modelType">
        /// The type of the model to "maximum".
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "maximum".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="maximumValue"/>,
        /// <paramref name="modelType"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Maximum(object maximumValue, Type modelType, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new MaximumTransform(maximumValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "minimum" transformation parameter for "minimum" transform commit writes to the <see cref="Builder"/>.
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
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="minimumValue"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Minimum<TModel>(object minimumValue, params string[] propertyNamePath)
            where TModel : class
        {
            fieldTransforms.Add(new MinimumTransform(minimumValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "minimum" transformation parameter for "minimum" transform commit writes to the <see cref="Builder"/>.
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
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="minimumValue"/>,
        /// <paramref name="modelType"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder Minimum(object minimumValue, Type modelType, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new MinimumTransform(minimumValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "removeAllFromArray" transformation parameter for "removeAllFromArray" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to "removeAllFromArray".
        /// </typeparam>
        /// <param name="removeAllFromArrayValue">
        /// The value to "removeAllFromArray" to the model <typeparamref name="TModel"/>.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "removeAllFromArray".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="removeAllFromArrayValue"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder RemoveAllFromArray<TModel>(IEnumerable<object> removeAllFromArrayValue, params string[] propertyNamePath)
            where TModel : class
        {
            fieldTransforms.Add(new RemoveAllFromArrayTransform(removeAllFromArrayValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "removeAllFromArray" transformation parameter for "removeAllFromArray" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="removeAllFromArrayValue">
        /// The value to "removeAllFromArray" to the model <paramref name="modelType"/>.
        /// </param>
        /// <param name="modelType">
        /// The type of the model to "removeAllFromArray".
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "removeAllFromArray".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="removeAllFromArrayValue"/>,
        /// <paramref name="modelType"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder RemoveAllFromArray(IEnumerable<object> removeAllFromArrayValue, Type modelType, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new RemoveAllFromArrayTransform(removeAllFromArrayValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to "setToServerValue".
        /// </typeparam>
        /// <param name="serverValue">
        /// The value to "setToServerValue" to the model <typeparamref name="TModel"/>.
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "setToServerValue".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder SetToServerValue<TModel>(ServerValue serverValue, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new SetToServerValueTransform(serverValue, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="serverValue">
        /// The value to "setToServerValue" to the model <paramref name="modelType"/>.
        /// </param>
        /// <param name="modelType">
        /// The type of the model to "setToServerValue".
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "setToServerValue".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="modelType"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder SetToServerValue(ServerValue serverValue, Type modelType, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new SetToServerValueTransform(serverValue, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <typeparam name="TModel">
        /// The type of the model to "setToServerValue".
        /// </typeparam>
        /// <param name="propertyNamePath">
        /// The property path of the model to "setToServerValue".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder SetToServerRequestTime<TModel>(params string[] propertyNamePath)
        {
            fieldTransforms.Add(new SetToServerValueTransform(ServerValue.RequestTime, typeof(TModel), propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds new field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes to the <see cref="Builder"/>.
        /// </summary>
        /// <param name="modelType">
        /// The type of the model to "setToServerValue".
        /// </param>
        /// <param name="propertyNamePath">
        /// The property path of the model to "setToServerValue".
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="modelType"/> or
        /// <paramref name="propertyNamePath"/> is a null reference.
        /// </exception>
        public Builder SetToServerRequestTime(Type modelType, params string[] propertyNamePath)
        {
            fieldTransforms.Add(new SetToServerValueTransform(ServerValue.RequestTime, modelType, propertyNamePath));
            return this;
        }

        /// <summary>
        /// Adds the <see cref="FieldTransform"/> to the builder.
        /// </summary>
        /// <param name="transform">
        /// The <see cref="FieldTransform"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="transform"/> is a null reference.
        /// </exception>
        public Builder Add(FieldTransform transform)
        {
            ArgumentNullException.ThrowIfNull(transform);

            fieldTransforms.Add(transform);
            return this;
        }

        /// <summary>
        /// Adds multiple the <see cref="FieldTransform"/> to the builder.
        /// </summary>
        /// <param name="transforms">
        /// The multiple of <see cref="FieldTransform"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="Builder"/> with new added transform.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="transforms"/> is a null reference.
        /// </exception>
        public Builder AddRange(IEnumerable<FieldTransform> transforms)
        {
            ArgumentNullException.ThrowIfNull(transforms);

            fieldTransforms.AddRange(transforms);
            return this;
        }

        /// <summary>
        /// Converts the <see cref="FieldTransform"/> to <see cref="Builder"/>
        /// </summary>
        /// <param name="transform">
        /// The <see cref="FieldTransform"/> to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="transform"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(FieldTransform transform)
        {
            return new Builder().Add(transform);
        }

        /// <summary>
        /// Converts the <see cref="FieldTransform"/> array to <see cref="Builder"/>
        /// </summary>
        /// <param name="transforms">
        /// The <see cref="FieldTransform"/> array to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="transforms"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(FieldTransform[] transforms)
        {
            return new Builder().AddRange(transforms);
        }

        /// <summary>
        /// Converts the <see cref="FieldTransform"/> list to <see cref="Builder"/>
        /// </summary>
        /// <param name="transforms">
        /// The <see cref="FieldTransform"/> list to convert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="transforms"/> is a null reference.
        /// </exception>
        public static implicit operator Builder(List<FieldTransform> transforms)
        {
            return new Builder().AddRange(transforms);
        }
    }

    /// <inheritdoc cref="Builder.AppendMissingElements{TModel}(IEnumerable{object}, string[])"/>
    public static Builder AppendMissingElements<TModel>(IEnumerable<object> appendMissingElementsValue, params string[] propertyNamePath)
        where TModel : class
    {
        return new Builder().AppendMissingElements<TModel>(appendMissingElementsValue, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.AppendMissingElements(IEnumerable{object}, Type, string[])"/>
    public static Builder AppendMissingElements(IEnumerable<object> appendMissingElementsValue, Type modelType, params string[] propertyNamePath)
    {
        return new Builder().AppendMissingElements(appendMissingElementsValue, modelType, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.Increment{TModel}(object, string[])"/>
    public static Builder Increment<TModel>(object incrementValue, params string[] propertyNamePath)
        where TModel : class
    {
        return new Builder().Increment<TModel>(incrementValue, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.Increment(object, Type, string[])"/>
    public static Builder Increment(object incrementValue, Type modelType, params string[] propertyNamePath)
    {
        return new Builder().Increment(incrementValue, modelType, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.Maximum{TModel}(object, string[])"/>
    public static Builder Maximum<TModel>(object maximumValue, params string[] propertyNamePath)
        where TModel : class
    {
        return new Builder().Maximum<TModel>(maximumValue, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.Maximum(object, Type, string[])"/>
    public static Builder Maximum(object maximumValue, Type modelType, params string[] propertyNamePath)
    {
        return new Builder().Maximum(maximumValue, modelType, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.Minimum{TModel}(object, string[])"/>
    public static Builder Minimum<TModel>(object minimumValue, params string[] propertyNamePath)
        where TModel : class
    {
        return new Builder().Minimum<TModel>(minimumValue, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.Minimum(object, Type, string[])"/>
    public static Builder Minimum(object minimumValue, Type modelType, params string[] propertyNamePath)
    {
        return new Builder().Minimum(minimumValue, modelType, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.RemoveAllFromArray{TModel}(IEnumerable{object}, string[])"/>
    public static Builder RemoveAllFromArray<TModel>(IEnumerable<object> removeAllFromArrayValue, params string[] propertyNamePath)
        where TModel : class
    {
        return new Builder().RemoveAllFromArray<TModel>(removeAllFromArrayValue, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.RemoveAllFromArray(IEnumerable{object}, Type, string[])"/>
    public static Builder RemoveAllFromArray(IEnumerable<object> removeAllFromArrayValue, Type modelType, params string[] propertyNamePath)
    {
        return new Builder().RemoveAllFromArray(removeAllFromArrayValue, modelType, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.SetToServerValue{TModel}(ServerValue, string[])"/>
    public static Builder SetToServerValue<TModel>(ServerValue serverValue, params string[] propertyNamePath)
    {
        return new Builder().SetToServerValue<TModel>(serverValue, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.SetToServerValue(ServerValue, Type, string[])"/>
    public static Builder SetToServerValue(ServerValue serverValue, Type modelType, params string[] propertyNamePath)
    {
        return new Builder().SetToServerValue(serverValue, modelType, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.SetToServerRequestTime{TModel}(string[])"/>
    public static Builder SetToServerRequestTime<TModel>(params string[] propertyNamePath)
    {
        return new Builder().SetToServerRequestTime<TModel>(propertyNamePath);
    }

    /// <inheritdoc cref="Builder.SetToServerRequestTime(Type, string[])"/>
    public static Builder SetToServerRequestTime(Type modelType, params string[] propertyNamePath)
    {
        return new Builder().SetToServerRequestTime(modelType, propertyNamePath);
    }

    /// <inheritdoc cref="Builder.Add(FieldTransform)"/>
    public static Builder Add(FieldTransform transform)
    {
        return new Builder().Add(transform);
    }

    /// <inheritdoc cref="Builder.AddRange(IEnumerable{FieldTransform})"/>
    public static Builder AddRange(IEnumerable<FieldTransform> transforms)
    {
        return new Builder().AddRange(transforms);
    }

    /// <summary>
    /// Gets the type of the model to transform.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
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
}
