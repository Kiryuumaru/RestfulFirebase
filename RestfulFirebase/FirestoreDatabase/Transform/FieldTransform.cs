using RestfulFirebase.FirestoreDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="AppendMissingElementsTransform.Create{TModel}(IEnumerable{object}, string[])"/>
        public Builder AppendMissingElements<TModel>(IEnumerable<object> appendMissingElementsValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(AppendMissingElementsTransform.Create<TModel>(appendMissingElementsValue, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="AppendMissingElementsTransform.Create(IEnumerable{object}, Type, string[])"/>
        public Builder AppendMissingElements(IEnumerable<object> appendMissingElementsValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(AppendMissingElementsTransform.Create(appendMissingElementsValue, modelType, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="IncrementTransform.Create{TModel}(object, string[])"/>
        public Builder Increment<TModel>(object incrementValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(IncrementTransform.Create<TModel>(incrementValue, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="IncrementTransform.Create(object, Type, string[])"/>
        public Builder Increment(object incrementValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(IncrementTransform.Create(incrementValue, modelType, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="MaximumTransform.Create{TModel}(object, string[])"/>
        public Builder Maximum<TModel>(object maximumValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(MaximumTransform.Create<TModel>(maximumValue, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="MaximumTransform.Create(object, Type, string[])"/>
        public Builder Maximum(object maximumValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(MaximumTransform.Create(maximumValue, modelType, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="MinimumTransform.Create{TModel}(object, string[])"/>
        public Builder Minimum<TModel>(object minimumValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(MinimumTransform.Create<TModel>(minimumValue, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="MinimumTransform.Create(object, Type, string[])"/>
        public Builder Minimum(object minimumValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(MinimumTransform.Create(minimumValue, modelType, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="RemoveAllFromArrayTransform.Create{TModel}(IEnumerable{object}, string[])"/>
        public Builder RemoveAllFromArray<TModel>(IEnumerable<object> removeAllFromArrayValue, params string[] propertyNamePath)
            where TModel : class
        {
            FieldTransforms.Add(RemoveAllFromArrayTransform.Create<TModel>(removeAllFromArrayValue, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="RemoveAllFromArrayTransform.Create(IEnumerable{object}, Type, string[])"/>
        public Builder RemoveAllFromArray(IEnumerable<object> removeAllFromArrayValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(RemoveAllFromArrayTransform.Create(removeAllFromArrayValue, modelType, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="SetToServerValueTransform.Create{TModel}(ServerValue, string[])"/>
        public Builder SetToServerValue<TModel>(ServerValue serverValue, params string[] propertyNamePath)
        {
            FieldTransforms.Add(SetToServerValueTransform.Create<TModel>(serverValue, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="SetToServerValueTransform.Create(ServerValue, Type, string[])"/>
        public Builder SetToServerValue(ServerValue serverValue, Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(SetToServerValueTransform.Create(serverValue, modelType, propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="SetToServerValueTransform.RequestTime{TModel}(string[])"/>
        public Builder SetToServerRequestTime<TModel>(params string[] propertyNamePath)
        {
            FieldTransforms.Add(SetToServerValueTransform.RequestTime<TModel>(propertyNamePath));
            return this;
        }

        /// <returns>
        /// The <see cref="Builder"/> added with new field transform.
        /// </returns>
        /// <inheritdoc cref="SetToServerValueTransform.RequestTime(Type, string[])"/>
        public Builder SetToServerRequestTime(Type modelType, params string[] propertyNamePath)
        {
            FieldTransforms.Add(SetToServerValueTransform.RequestTime(modelType, propertyNamePath));
            return this;
        }
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
