using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.References;
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
