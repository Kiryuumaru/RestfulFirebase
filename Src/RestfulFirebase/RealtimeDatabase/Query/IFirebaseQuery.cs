namespace RestfulFirebase.RealtimeDatabase.Query;

using System;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.RealtimeDatabase.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System.Collections.Generic;

/// <summary>
/// The base declaration for firebase query operations.
/// </summary>
public interface IFirebaseQuery
{
    /// <summary>
    /// Gets the underlying <see cref="RestfulFirebaseApp"/> the module uses.
    /// </summary>
    RestfulFirebaseApp App { get; }

    /// <summary>
    /// Gets the underlying <see cref="RestfulFirebaseApp"/> the module uses.
    /// </summary>
    RealtimeDatabase RealtimeDatabase { get; }

    /// <summary>
    /// Creates new instance of <see cref="RealtimeWire"/> at the given query location.
    /// </summary>
    /// <returns>
    /// The created <see cref="RealtimeWire"/> of the query location.
    /// </returns>
    RealtimeWire AsRealtimeWire();

    /// <summary>
    /// Builds the url of the query.
    /// </summary>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> of the created <see cref="Task"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the built URL.
    /// </returns>
    /// <exception cref="AuthAPIKeyNotValidException">
    /// API key not valid. Please pass a valid API key.
    /// </exception>
    /// <exception cref="AuthTokenExpiredException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthUserDisabledException">
    /// The user account has been disabled by an administrator.
    /// </exception>
    /// <exception cref="AuthUserNotFoundException">
    /// The user corresponding to the refresh token was not found. It is likely the user was deleted.
    /// </exception>
    /// <exception cref="AuthInvalidIDTokenException">
    /// The user's credential is no longer valid. The user must sign in again.
    /// </exception>
    /// <exception cref="AuthInvalidRefreshTokenException">
    /// An invalid refresh token is provided.
    /// </exception>
    /// <exception cref="AuthInvalidJSONReceivedException">
    /// Invalid JSON payload received.
    /// </exception>
    /// <exception cref="AuthMissingRefreshTokenException">
    /// No refresh token provided.
    /// </exception>
    /// <exception cref="AuthUndefinedException">
    /// The error occured is undefined.
    /// </exception>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The operation was cancelled.
    /// </exception>
    Task<string> BuildUrl(CancellationToken? token = null);

    /// <summary>
    /// Creates new instance of <see cref="ChildQuery"/> node with the specified child <paramref name="path"/>.
    /// </summary>
    /// <param name="path">
    /// The resource name of the node.
    /// </param>
    /// <returns>
    /// The created <see cref="ChildQuery"/> node.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Throws when <paramref name="path"/> is null or empty.
    /// </exception>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when <paramref name="path"/> has forbidden node name character.
    /// </exception>
    ChildQuery Child(string path);

    /// <summary>
    /// Fan out given item to multiple locations at once. See https://firebase.googleblog.com/2015/10/client-side-fan-out-for-data-consistency_73.html for details.
    /// </summary>
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="pathValues">
    /// The json data values to fan out.
    /// </param>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed fan out <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed fan out <see cref="Task"/>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when <paramref name="pathValues"/> has forbidden node name character.
    /// </exception>
    Task<bool> FanOut(IDictionary<string, string> pathValues, CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);

    /// <summary>
    /// Fan out given item to multiple locations at once. See https://firebase.googleblog.com/2015/10/client-side-fan-out-for-data-consistency_73.html for details.
    /// </summary>
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="jsonData">
    /// The json data to fan out.
    /// </param>
    /// <param name="relativePaths">
    /// Locations where to store the data.
    /// </param>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed fan out <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed fan out <see cref="Task"/>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when <paramref name="relativePaths"/> has forbidden node name character.
    /// </exception>
    Task<bool> FanOut(string? jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);

    /// <summary>
    /// Fan out given item to multiple locations at once. See https://firebase.googleblog.com/2015/10/client-side-fan-out-for-data-consistency_73.html for details.
    /// </summary>
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="jsonData">
    /// The json data to fan out.
    /// </param>
    /// <param name="relativePaths">
    /// Locations where to store the data.
    /// </param>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed fan out <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed fan out <see cref="Task"/>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when <paramref name="relativePaths"/> has forbidden node name character.
    /// </exception>
    Task<bool> FanOut(Func<string?> jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);

    /// <summary>
    /// Gets the json data of the given location.
    /// </summary>
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed get <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed get <see cref="Task"/>.
    /// </returns>
    Task<string?> Get(CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);

    /// <summary>
    /// Gets the absolute path of the query.
    /// </summary>
    /// <returns>
    /// The absolute path of the query.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    string GetAbsoluteUrl();

    /// <summary>
    /// Patches data at given location instead of overwriting them.
    /// </summary> 
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="jsonData">
    /// The json data to patch.
    /// </param>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed patch <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed patch <see cref="Task"/>.
    /// </returns>
    Task<bool> Patch(string? jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);

    /// <summary>
    /// Patches data at given location instead of overwriting them.
    /// </summary> 
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="jsonData">
    /// The json data to patch.
    /// </param>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed patch <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed patch <see cref="Task"/>.
    /// </returns>
    Task<bool> Patch(Func<string?> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);

    /// <summary>
    /// Puts or overrides data at the given location.
    /// </summary>
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="jsonData">
    /// The json data to put.
    /// </param>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed put <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed put <see cref="Task"/>.
    /// </returns>
    Task<bool> Put(string? jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);

    /// <summary>
    /// Puts or overrides data at the given location.
    /// </summary>
    /// <remarks>
    /// <para>Possible exceptions for callback <paramref name="onException"/>:</para>
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
    /// <para><see cref="AuthUndefinedException"/> - The error occured is undefined.</para>
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    /// <param name="jsonData">
    /// The json data to put.
    /// </param>
    /// <param name="token">
    /// The <see cref="CancellationToken"/> for the executed put <see cref="Task"/>.
    /// </param>
    /// <param name="onException">
    /// The callback for failed operations.
    /// </param>
    /// <returns>
    /// The created <see cref="Task"/> represents the executed put <see cref="Task"/>.
    /// </returns>
    Task<bool> Put(Func<string?> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs>? onException = null);
}
