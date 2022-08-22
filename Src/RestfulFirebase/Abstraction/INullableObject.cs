﻿using System;

namespace RestfulFirebase.Abstraction;

/// <summary>
/// Contains declarations for implicit nullable object.
/// </summary>
public interface INullableObject1 :
    IDisposable
{
    /// <summary>
    /// Sets this object to null.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this object sets to null; otherwise, <c>false</c>.
    /// </returns>
    bool SetNull();

    /// <summary>
    /// Check if object is null.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this object is null; otherwise, <c>false</c>.
    /// </returns>
    bool IsNull();
}
