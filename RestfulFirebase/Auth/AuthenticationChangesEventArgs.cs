using System;

namespace RestfulFirebase.Auth;

/// <summary>
/// Event arguments for authentiation changes invokes.
/// </summary>
public class AuthenticationChangesEventArgs : EventArgs
{
    /// <summary>
    /// Gets <c>true</c> whether the user is authenticated; otherwise <c>false</c>.
    /// </summary>
    public bool IsAuthenticated { get; }

    internal AuthenticationChangesEventArgs(bool isAuthenticated)
    {
        IsAuthenticated = isAuthenticated;
    }
}
