using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// Supports cloning, which creates a new instance of a class with the same value.
/// </summary>
public interface ICloneable<TClonable> : ICloneable
{
    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    new TClonable Clone();
}
