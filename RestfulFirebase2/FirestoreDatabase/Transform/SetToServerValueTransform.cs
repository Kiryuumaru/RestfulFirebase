using RestfulFirebase.FirestoreDatabase.Enums;
using System;

namespace RestfulFirebase.FirestoreDatabase.Transform;

/// <summary>
/// The field "setToServerValue" transformation parameter for "setToServerValue" transform commit writes.
/// </summary>
public class SetToServerValueTransform : FieldTransform
{
    /// <summary>
    /// Gets the object to "setToServerValue" to the given property path.
    /// </summary>
    public ServerValue ServerValue { get; }

    /// <summary>
    /// Creates new instance of <see cref="SetToServerValueTransform"/>.
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
    /// <exception cref="ArgumentNullException">
    /// <paramref name="modelType"/> or
    /// <paramref name="propertyNamePath"/> is a null reference.
    /// </exception>
    public SetToServerValueTransform(ServerValue serverValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ServerValue = serverValue;
    }
}
