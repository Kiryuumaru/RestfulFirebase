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

    internal SetToServerValueTransform(ServerValue serverValue, Type modelType, string[] propertyNamePath)
        : base(modelType, propertyNamePath)
    {
        ServerValue = serverValue;
    }
}
