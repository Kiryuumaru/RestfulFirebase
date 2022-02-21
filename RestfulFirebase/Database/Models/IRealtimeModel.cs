using ObservableHelpers;
using ObservableHelpers.Abstraction;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models;

/// <summary>
/// Provides realtime observable model for <see cref="Realtime.RealtimeInstance"/>
/// </summary>
public interface IRealtimeModel : IObservable
{
    /// <summary>
    /// Gets a value indicating whether this object is in the process of disposing.
    /// </summary>
    public bool IsDisposing { get; }

    /// <summary>
    /// Gets a value indicating whether this object has been disposed.
    /// </summary>
    public bool IsDisposed { get; }

    /// <summary>
    /// Gets a value indicating whether this object has been disposed or is in the process of being disposed.
    /// </summary>
    public bool IsDisposedOrDisposing { get; }

    /// <summary>
    /// Gets the <see cref="Realtime.RealtimeInstance"/> the model uses.
    /// </summary>
    RealtimeInstance? RealtimeInstance { get; }

    /// <summary>
    /// Gets <c>true</c> whether model has realtime instance attached; otherwise, <c>false</c>.
    /// </summary>
    bool HasAttachedRealtime { get; }

    /// <summary>
    /// Event raised on current context if the realtime instance is attached on the model.
    /// </summary>
    event EventHandler<RealtimeInstanceEventArgs>? RealtimeAttached;

    /// <summary>
    /// Event raised on current context if the realtime instance is detached on the model.
    /// </summary>
    event EventHandler<RealtimeInstanceEventArgs>? RealtimeDetached;

    /// <summary>
    /// Event raised on current context if the realtime instance encounters an error.
    /// </summary>
    /// <remarks>
    /// <para>Possible Exceptions:</para>
    /// <para><see cref="OfflineModeException"/> - Offline mode is enabled.</para>
    /// <para><see cref="DatabaseException"/> - A realtime database exception has occured.</para>
    /// <para><see cref="DatabaseInternalServerErrorException"/> - An internal server error occured.</para>
    /// <para><see cref="DatabaseNotFoundException"/> - The specified Realtime Database was not found.</para>
    /// <para><see cref="DatabasePreconditionFailedException"/> - The request's specified ETag value in the if-match header did not match the server's value.</para>
    /// <para><see cref="DatabaseServiceUnavailableException"/> - The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.</para>
    /// <para><see cref="DatabaseUnauthorizedException"/> - The request is not authorized by database rules.</para>
    /// <para><see cref="DatabaseUndefinedException"/> - An unidentified error occured.</para>
    /// <para><see cref="AuthException"/> - An authentication exception has occured.</para>
    /// <para><see cref="AuthAPIKeyNotValidException"/> - API key not valid. Please pass a valid API key.</para>
    /// <para><see cref="AuthTokenExpiredException"/> - The user's credential is no longer valid. The user must sign in again.</para>
    /// <para><see cref="AuthUserDisabledException"/> - The user account has been disabled by an administrator.</para>
    /// <para><see cref="AuthUserNotFoundException"/> - The user corresponding to the refresh token was not found. It is likely the user was deleted.</para>
    /// <para><see cref="AuthInvalidIDTokenException"/> - The user's credential is no longer valid. The user must sign in again.</para>
    /// <para><see cref="AuthInvalidRefreshTokenException"/> - An invalid refresh token is provided.</para>
    /// <para><see cref="AuthInvalidJSONReceivedException"/> - Invalid JSON payload received.</para>
    /// <para><see cref="AuthMissingRefreshTokenException"/> - No refresh token provided.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    event EventHandler<WireExceptionEventArgs>? WireError;
}
