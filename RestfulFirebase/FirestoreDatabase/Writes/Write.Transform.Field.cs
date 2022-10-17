using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Writes;

/// <summary>
/// The field transformation parameter for transform commit writes.
/// </summary>
public abstract class FieldTransform
{
    /// <summary>
    /// Gets the path of the property or document field to filter.
    /// </summary>
    public string[] NamePath { get; internal set; }

    /// <summary>
    /// Gets <c>true</c> if the <see cref="NamePath"/> is a property name; otherwise <c>false</c> if it is a document field name.
    /// </summary>
    public bool IsNamePathAPropertyPath { get; internal set; }

    internal FieldTransform(string[] namePath, bool isPathPropertyName)
    {
        NamePath = namePath;
        IsNamePathAPropertyPath = isPathPropertyName;
    }
}
