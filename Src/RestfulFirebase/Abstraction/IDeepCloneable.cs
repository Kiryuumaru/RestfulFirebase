using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Abstraction;

/// <summary>
/// Generic interface for a deeply cloneable type.
/// </summary>
/// <typeparam name="T">
/// The type itself.
/// </typeparam>
public interface IDeepCloneable<T>
{
    /// <summary>
    /// Creates a deep clone of this object.
    /// </summary>
    /// <returns>
    /// A deep clone of this object.
    /// </returns>
    T Clone();
}
