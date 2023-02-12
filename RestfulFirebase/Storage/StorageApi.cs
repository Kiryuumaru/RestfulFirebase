using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Storage;

/// <summary>
/// Provides firebase storage implementations.
/// </summary>
public partial class StorageApi
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal StorageApi(FirebaseApp app)
    {
        App = app;
    }
}