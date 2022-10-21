using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The base fluent request for firestore database.
/// </summary>
public abstract partial class FluentRequest
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    internal FluentRequest(FirebaseApp app)
    {
        App = app;
    }
}
