﻿namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public partial class AuthenticationApi
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    internal FirebaseApp App { get; }

    internal AuthenticationApi(FirebaseApp app)
    {
        App = app;
    }
}
