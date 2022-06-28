using DisposableHelpers;
using LockerHelpers;
using RestfulFirebase.Abstraction;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.RealtimeDatabase.Realtime;
using SerializerHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RestfulFirebase.RealtimeDatabase.Models;

/// <summary>
/// Provides realtime base model for <see cref="Realtime.RealtimeInstance"/>
/// </summary>
public abstract class RealtimeModel : Disposable
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="Realtime.RealtimeInstance"/> the model uses.
    /// </summary>
    public RealtimeInstance? RealtimeInstance { get; private set; }

    /// <summary>
    /// Gets <c>true</c> whether model has realtime instance attached; otherwise, <c>false</c>.
    /// </summary>
    public bool HasAttachedRealtime { get => !(RealtimeInstance?.IsDisposed ?? true); }

    /// <summary>
    /// Gets the read-write lock for concurrency.
    /// </summary>
    public RWLock RWLock { get; } = new RWLock(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Event raised on current context if the realtime instance is attached on the model.
    /// </summary>
    public event EventHandler<RealtimeInstanceEventArgs>? RealtimeAttached;

    /// <summary>
    /// Event raised on current context if the realtime instance is detached on the model.
    /// </summary>
    public event EventHandler<RealtimeInstanceEventArgs>? RealtimeDetached;

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
    public event EventHandler<WireExceptionEventArgs>? WireError;

    internal bool? IsInvokeToSetFirst { get; private set; }

    internal bool HasPostAttachedRealtime { get; private set; }

    #endregion

    #region Initializers



    #endregion

    #region Methods

    /// <summary>
    /// Invokes <see cref="RealtimeAttached"/> event on the current context.
    /// </summary>
    /// <param name="args">
    /// The event arguments for the event to invoke.
    /// </param>
    protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
    {
        RealtimeAttached?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes <see cref="RealtimeDetached"/> event on the current context.
    /// </summary>
    /// <param name="args">
    /// The event arguments for the event to invoke.
    /// </param>
    protected virtual void OnRealtimeDetached(RealtimeInstanceEventArgs args)
    {
        RealtimeDetached?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes <see cref="WireError"/> event on the current context.
    /// </summary>
    /// <param name="args">
    /// The event arguments for the event to invoke.
    /// </param>
    protected virtual void OnWireError(WireExceptionEventArgs args)
    {
        WireError?.Invoke(this, args);
    }

    internal abstract void RealtimeInstanceAttaching();

    internal abstract void RealtimeInstanceDetaching();

    internal abstract void RealtimeInstanceDataChanges(DataChangesEventArgs e);

    internal void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
    {
        if (IsDisposed && !realtimeInstance.IsDisposed)
        {
            return;
        }

        HasPostAttachedRealtime = true;

        RWLock.LockWriteAndForget(() =>
        {
            try
            {
                if (IsDisposed && !realtimeInstance.IsDisposed)
                {
                    return;
                }

                RealtimeInstanceAttaching();

                Subscribe(realtimeInstance, invokeSetFirst);

                RWLock.InvokeOnLockExit(() => OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance)));
            }
            catch
            {
                Unsubscribe();

                throw;
            }
            finally
            {
                HasPostAttachedRealtime = false;
            }
        });
    }

    internal void DetachRealtime()
    {
        if (IsDisposed || !HasAttachedRealtime)
        {
            return;
        }

        RWLock.LockWriteAndForget(() =>
        {
            if (RealtimeInstance == null)
            {
                return;
            }

            RealtimeInstanceDetaching();

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            RWLock.InvokeOnLockExit(() => OnRealtimeDetached(args));
        });
    }

    private void Subscribe(RealtimeInstance realtimeInstance, bool invokeSetFirst)
    {
        if (IsDisposed || realtimeInstance.IsDisposed)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            if (IsDisposed || realtimeInstance.IsDisposed)
            {
                return;
            }

            if (HasAttachedRealtime)
            {
                Unsubscribe();
            }

            RealtimeInstance = realtimeInstance;
            IsInvokeToSetFirst = invokeSetFirst;

            RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
            RealtimeInstance.Error += RealtimeInstance_Error;
            RealtimeInstance.Disposing += RealtimeInstance_Disposing;
        });
    }

    private void Unsubscribe()
    {
        if (IsDisposed || !HasAttachedRealtime)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            if (RealtimeInstance == null)
            {
                return;
            }

            RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
            RealtimeInstance.Error -= RealtimeInstance_Error;
            RealtimeInstance.Disposing -= RealtimeInstance_Disposing;

            RealtimeInstance = null;
            IsInvokeToSetFirst = null;
        });
    }

    private void RealtimeInstance_DataChanges(object? sender, DataChangesEventArgs e)
    {
        if (IsDisposed || !HasAttachedRealtime)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            if (!HasAttachedRealtime)
            {
                return;
            }

            RealtimeInstanceDataChanges(e);
        });
    }

    private void RealtimeInstance_Error(object? sender, WireExceptionEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        OnWireError(e);
    }

    private void RealtimeInstance_Disposing(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        DetachRealtime();
    }

    #endregion
}
