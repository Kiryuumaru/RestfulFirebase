using LockerHelpers;
using ObservableHelpers;
using ObservableHelpers.Abstraction;
using ObservableHelpers.Utilities;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using SerializerHelpers;
using SynchronizationContextHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Realtime;

/// <summary>
/// Provides fluid implementations for firebase realtime database.
/// </summary>
public class RealtimeInstance : SyncContext, INullableObject, ICloneable
{
    #region Properties

    /// <summary>
    /// Gets the underlying <see cref="RestfulFirebaseApp"/> the module uses.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    /// <summary>
    /// The firebase query of the instance.
    /// </summary>
    public IFirebaseQuery Query { get; }

    /// <summary>
    /// The root <see cref="RealtimeWire"/> of the instance.
    /// </summary>
    public RealtimeWire? Root { get; }

    /// <summary>
    /// The parent <see cref="RealtimeInstance"/> of the instance.
    /// </summary>
    public RealtimeInstance? Parent { get; }

    /// <summary>
    /// Local database used by this realtime instance.
    /// </summary>
    public ILocalDatabase LocalDatabase { get; }

    /// <summary>
    /// Gets <c>true</c> whether the wire has first stream since creation; otherwise, <c>false</c>.
    /// </summary>
    public virtual bool HasFirstStream => Root?.HasFirstStream ?? false;

    /// <summary>
    /// Gets <c>true</c> whether the wire has started the node subscription; otherwise, <c>false</c>.
    /// </summary>
    public virtual bool Started => Root?.Started ?? false;

    /// <summary>
    /// Event raised on the current context when there is data changes on the node or sub nodes.
    /// </summary>
    public event EventHandler<DataChangesEventArgs>? DataChanges;

    /// <summary>
    /// Event raised when there is data changes on the node or sub nodes.
    /// </summary>
    public event EventHandler<DataChangesEventArgs>? ImmediateDataChanges;

    /// <summary>
    /// Event raised on the current context when there is an error occured.
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
    /// <para><see cref="DatabaseForbiddenNodeNameCharacter"/> - Throws when any node has forbidden node name character.</para>
    /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
    /// </remarks>
    public event EventHandler<WireExceptionEventArgs>? Error;

    internal readonly OperationInvoker writeTaskErrorControl = new(1);

    internal string AbsoluteUrl { get; private set; }

    internal string[] DBPath { get; private set; }

    #endregion

    #region Initializers

    private protected RealtimeInstance(RestfulFirebaseApp app, IFirebaseQuery query, ILocalDatabase localDatabase)
    {
        App = app;
        Query = query;
        LocalDatabase = localDatabase;
        App.Disposing += App_Disposing;

        AbsoluteUrl = Query.GetAbsoluteUrl();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        DBPath = UrlUtilities.Separate(AbsoluteUrl[8..]); // Removes 'https://'
#else
    DBPath = UrlUtilities.Separate(AbsoluteUrl[8..]); // Removes 'https://'
#endif

        App.LocalDatabase.InternalSubscribe(LocalDatabase, OnDataChanges);
    }

    private protected RealtimeInstance(RestfulFirebaseApp app, RealtimeWire? root, RealtimeInstance? parent, IFirebaseQuery query, ILocalDatabase localDatabase)
       : this(app, query, localDatabase)
    {
        Root = root;
        Parent = parent;

        if (Root != null)
        {
            Root.Error += Parent_Error;
        }
        if (Parent != null)
        {
            Parent.Disposing += Parent_Disposing;
        }
    }

    private protected RealtimeInstance(RestfulFirebaseApp app, RealtimeWire? root, RealtimeInstance parent, string path, ILocalDatabase localDatabase)
       : this(app, root, parent, parent.Query.Child(path), localDatabase)
    {

    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a <see cref="Task"/> that will complete when the instance is fully synced.
    /// </summary>
    /// <param name="timeout">
    /// The <see cref="TimeSpan"/> timeout of the created task.
    /// </param>
    /// <param name="path">
    /// The path of the data to wait for sync.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the fully sync status.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    public Task<bool> WaitForSynced(TimeSpan timeout, params string[] path)
    {
        return WaitForSynced(true, new CancellationTokenSource(timeout).Token, path);
    }

    /// <summary>
    /// Creates a <see cref="Task"/> that will complete when the instance is fully synced.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> for the wait synced status.
    /// </param>
    /// <param name="path">
    /// The path of the data to wait for sync.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the fully sync status.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    public Task<bool> WaitForSynced(CancellationToken cancellationToken, params string[] path)
    {
        return WaitForSynced(true, cancellationToken, path);
    }

    /// <summary>
    /// Creates a <see cref="Task"/> that will complete when the instance is fully synced.
    /// </summary>
    /// <param name="cancelOnError">
    /// Specify <c>true</c> whether the task will be cancelled on error; otherwise, <c>false</c>.
    /// </param>
    /// <param name="timeout">
    /// The <see cref="TimeSpan"/> timeout of the created task.
    /// </param>
    /// <param name="path">
    /// The path of the data to wait for sync.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the fully sync status.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    public Task<bool> WaitForSynced(bool cancelOnError, TimeSpan timeout, params string[] path)
    {
        return WaitForSynced(cancelOnError, new CancellationTokenSource(timeout).Token, path);
    }

    /// <summary>
    /// Creates a <see cref="Task"/> that will complete when the instance is fully synced.
    /// </summary>
    /// <param name="cancelOnError">
    /// Specify <c>true</c> whether the task will be cancelled on error; otherwise, <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> for the wait synced status.
    /// </param>
    /// <param name="path">
    /// The path of the data to wait for sync.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the fully sync status.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public async Task<bool> WaitForSynced(bool cancelOnError = true, CancellationToken? cancellationToken = null, params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        bool cancel = false;
        void RealtimeInstance_Error(object? sender, WireExceptionEventArgs e)
        {
            cancel = true;
        }
        if (cancelOnError)
        {
            Error += RealtimeInstance_Error;
        }
        async Task<bool> waitTask()
        {
            while (!IsSynced(path) && !App.Config.CachedOfflineMode && !cancel && !(cancellationToken?.IsCancellationRequested ?? false))
            {
                try
                {
                    if (cancellationToken.HasValue)
                    {
                        await Task.Delay(App.Config.CachedDatabaseRetryDelay, cancellationToken.Value).ConfigureAwait(false);
                    }
                    else
                    {
                        await Task.Delay(App.Config.CachedDatabaseRetryDelay).ConfigureAwait(false);
                    }
                }
                catch { }
            }
            return IsSynced(path);
        }
        bool result = await Task.Run(waitTask).ConfigureAwait(false);
        if (cancelOnError)
        {
            Error -= RealtimeInstance_Error;
        }
        return result;
    }

    /// <summary>
    /// Gets the data count of the instance.
    /// </summary>
    /// <param name="path">
    /// The path of the data and/or sub data to count.
    /// </param>
    /// <returns>
    /// The total and sync data count of the <paramref name="path"/>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public (int total, int synced) GetDataCount(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        int total = 0;
        int syned = 0;
        (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] children = InternalGetRecursiveRelativeData(path);
        foreach ((string[] childPath, string? sync, string? local, string? value, LocalDataChangesType changesType) in children)
        {
            total++;
            if (changesType == LocalDataChangesType.Synced)
            {
                syned++;
            }
        }
        return (total, syned);
    }

    /// <summary>
    /// Gets <c>true</c> whether the node is fully synced; otherwise, <c>false</c>.
    /// </summary>
    /// <param name="path">
    /// The path of the data to check.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the node is fully synced; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public bool IsSynced(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        if (HasFirstStream)
        {
            (int total, int synced) = GetDataCount(path);
            return total == synced;
        }

        return false;
    }

    /// <summary>
    /// Gets <c>true</c> whether the node is locally available; otherwise, <c>false</c>.
    /// </summary>
    /// <param name="path">
    /// The path of the data to check.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the node is locally available; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public bool IsLocallyAvailable(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        (int total, _) = GetDataCount(path);
        return total != 0;
    }

    /// <summary>
    /// Writes and subscribes realtime model to the node instance.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the realtime model.
    /// </typeparam>
    /// <param name="model">
    /// The realtime model to write and subscribe.
    /// </param>
    /// <param name="path">
    /// The path of the instance to attach.
    /// </param>
    /// <returns>
    /// The provided <paramref name="model"/>.
    /// </returns>
    /// <exception cref="DatabaseInvalidModel">
    /// Throws when <paramref name="model"/> is not a valid model.
    /// </exception>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public void PutModel<T>(T model, params string[] path)
        where T : IRealtimeModel
    {
        VerifyNotDisposed();

        if (model.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(T));
        }

        if (model is IInternalRealtimeModel internalModel)
        {
            if (path == null || path.Length == 0)
            {
                internalModel.AttachRealtime(this, true);
            }
            else
            {
                EnsureValidPath(path);
                internalModel.AttachRealtime(Child(path), true);
            }
        }
        else
        {
            throw new DatabaseInvalidModel();
        }
    }

    /// <summary>
    /// Subscribes realtime model to the node instance.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the realtime model.
    /// </typeparam>
    /// <param name="model">
    /// The realtime model to subscribe.
    /// </param>
    /// <param name="path">
    /// The path of the instance to attach.
    /// </param>
    /// <returns>
    /// The provided <paramref name="model"/>.
    /// </returns>
    /// <exception cref="DatabaseInvalidModel">
    /// Throws when <paramref name="model"/> is not a valid model.
    /// </exception>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public void SubModel<T>(T model, params string[] path)
        where T : IRealtimeModel
    {
        VerifyNotDisposed();

        if (model.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(T));
        }

        if (model is IInternalRealtimeModel internalModel)
        {
            if (path == null || path.Length == 0)
            {
                internalModel.AttachRealtime(this, false);
            }
            else
            {
                EnsureValidPath(path);
                internalModel.AttachRealtime(Child(path), false);
            }
        }
        else
        {
            throw new DatabaseInvalidModel();
        }
    }

    /// <summary>
    /// Check whether the specified <paramref name="instance"/> is a sub path from this instance.
    /// </summary>
    /// <param name="instance">
    /// </param>
    /// <returns>
    /// <c>true</c> whether the specified <paramref name="instance"/> is a sub path from this instance; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Throws when <paramref name="instance"/> is a null reference.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public bool IsSubPath(RealtimeInstance instance)
    {
        VerifyNotDisposed();

        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        if (instance.DBPath.Length <= DBPath.Length)
        {
            return false;
        }

        for (int i = 0; i < DBPath.Length; i++)
        {
            if (DBPath[i] != instance.DBPath[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Invokes <see cref="Error"/> event to instance and parent instance.
    /// </summary>
    /// <param name="uri">
    /// The affected uri of the error.
    /// </param>
    /// <param name="exception">
    /// The exception of the error.
    /// </param>
    protected void OnError(string uri, Exception exception)
    {
        if (IsDisposed)
        {
            return;
        }

        if (Root == null)
        {
            SelfError(new WireExceptionEventArgs(uri, exception));
        }
        else
        {
            Root.OnError(uri, exception);
        }
    }

    private void Parent_Error(object? sender, WireExceptionEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        string baseUri = AbsoluteUrl.Trim('/');
        if (e.Uri.StartsWith(baseUri))
        {
            SelfError(e);
        }
    }

    private void Parent_Disposing(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        Dispose();
    }

    private void OnDataChanges(object? sender, DataChangesEventArgs args)
    {
        if (IsDisposed)
        {
            return;
        }

        if (DBPath.Length > args.Path.Length - 1)
        {
            return;
        }
        for (int i = 0; i < DBPath.Length; i++)
        {
            if (DBPath[i] != args.Path[i + 1])
            {
                return;
            }
        }

        string[] path = new string[args.Path.Length - 1 - DBPath.Length];
        if (path.Length != 0)
        {
            Array.Copy(args.Path, args.Path.Length - path.Length, path, 0, path.Length);
        }

        DataChangesEventArgs dataChangesArgs = new(path);

        ImmediateDataChanges?.Invoke(this, dataChangesArgs);
        ContextPost(delegate
        {
            DataChanges?.Invoke(this, dataChangesArgs);
        });
    }

    private void SelfError(WireExceptionEventArgs e)
    {
        ContextPost(delegate
        {
            Error?.Invoke(this, e);
        });
    }

    private void App_Disposing(object? sender, EventArgs e)
    {
        App.Disposing -= App_Disposing;
        Dispose();
    }

    #endregion

    #region Fluid Methods

    /// <summary>
    /// Creates new child instance with the provided <paramref name="path"/>.
    /// </summary>
    /// <param name="path">
    /// The path of the child instance to create.
    /// </param>
    /// <returns>
    /// The created child instance.
    /// </returns>
    /// <exception cref="StringNullOrEmptyException">
    /// <paramref name="path"/> has null or empty path.
    /// </exception>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public RealtimeInstance Child(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidAndNonEmptyPath(path);

        RealtimeInstance childWire;
        if (Root == null && this is RealtimeWire wire)
        {
            childWire = new RealtimeInstance(App, wire, this, UrlUtilities.Combine(path), LocalDatabase);
        }
        else
        {
            childWire = new RealtimeInstance(App, Root, this, UrlUtilities.Combine(path), LocalDatabase);
        }

        childWire.SyncOperation.SetContext(this);

        return childWire;
    }

    /// <summary>
    /// Creates a clone of the instance.
    /// </summary>
    /// <returns>
    /// The created clone of the instance.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public virtual RealtimeInstance Clone()
    {
        VerifyNotDisposed();

        var clone = new RealtimeInstance(App, Root, Parent, Query, LocalDatabase);
        clone.SyncOperation.SetContext(this);

        return clone;
    }

    /// <summary>
    /// Checks whether the specified <paramref name="path"/> has any children.
    /// </summary>
    /// <param name="path">
    /// The path of the child to check.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the specified <paramref name="path"/> has any children; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public bool HasChildren(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        return InternalGetDataType(path) == LocalDataType.Path;
    }

    #endregion

    #region CRUD Methods

    /// <summary>
    /// Gets children of the instance.
    /// </summary>
    /// <param name="path">
    /// The path of the sub paths.
    /// </param>
    /// <returns>
    /// The all children.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public (string key, LocalDataType type)[] GetChildren(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        return InternalGetChildren(path);
    }

    /// <summary>
    /// Gets the data of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">
    /// The path of the data to get.
    /// </param>
    /// <returns>
    /// The data of the <paramref name="path"/>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public (string? sync, string? local, string? value, LocalDataChangesType changesType)? GetData(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        return InternalGetData(path);
    }

    /// <summary>
    /// Gets all recursive children of the instance.
    /// </summary>
    /// <param name="path">
    /// The path of the sub paths.
    /// </param>
    /// <returns>
    /// The all recursive children.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public string[][] GetRecursiveChildren(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        return InternalGetRecursiveRelativeChildren(path);
    }

    /// <summary>
    /// Gets all sub data of the instance.
    /// </summary>
    /// <param name="path">
    /// The path of the sub data.
    /// </param>
    /// <returns>
    /// The all sub data.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] GetRecursiveData(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        return InternalGetRecursiveRelativeData(path);
    }

    /// <summary>
    /// Gets the value of the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">
    /// The path of the value to get.
    /// </param>
    /// <returns>
    /// The value of the <paramref name="path"/>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public string? GetValue(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        var data = InternalGetData(path);
        if (data.HasValue)
        {
            return data.Value.value;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Check if object is null.
    /// </summary>
    /// <param name="path">
    /// The path of the value to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if this object is null; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public bool IsNull(params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        bool isNull = true;

        InternalGetDataOrRecursiveRelativeChildren(
            delegate { },
            data =>
            {
                if (data.value != null)
                {
                    isNull = false;
                }
            },
            children =>
            {
                if (children != null && children.Length != 0)
                {
                    isNull = false;
                }
            }, path);

        return isNull;
    }

    /// <summary>
    /// Sets this object to null.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this object sets to null; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public bool SetNull(params string[] path)
    {
        return SetValue(null, path);
    }

    /// <summary>
    /// Sets value to the specified  <paramref name="path"/>.
    /// </summary>
    /// <param name="value">
    /// The value to set.
    /// </param>
    /// <param name="path">
    /// The path of the value to set.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the value was set; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="DatabaseForbiddenNodeNameCharacter">
    /// Throws when any node has forbidden node name character.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Throws when the object is already disposed.
    /// </exception>
    public bool SetValue(string? value, params string[] path)
    {
        VerifyNotDisposed();

        EnsureValidPath(path);

        return MakeChanges(value, path);
    }

    private static void EnsureValidPath(string[] path)
    {
        if (path == null || path.Length == 0)
        {
            return;
        }
        foreach (string node in path)
        {
            if (string.IsNullOrEmpty(node))
            {
                throw StringNullOrEmptyException.FromEnumerableArgument(nameof(path));
            }
            else if (node.Any(
                c =>
                {
                    switch (c)
                    {
                        case '$': return true;
                        case '#': return true;
                        case '[': return true;
                        case ']': return true;
                        case '.': return true;
                        default:
                            if ((c >= 0 && c <= 31) || c == 127)
                            {
                                return true;
                            }
                            return false;
                    }
                }))
            {
                throw new DatabaseForbiddenNodeNameCharacter();
            }
        }
    }

    private static void EnsureValidAndNonEmptyPath(string[] path)
    {
        if (path == null || path.Length == 0)
        {
            throw StringNullOrEmptyException.FromSingleArgument(nameof(path));
        }
        EnsureValidPath(path);
    }

    #endregion

    #region Internal Helpers

    internal (string key, LocalDataType type)[] InternalGetChildren(params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        return DBGetRelativeTypedChildren(absoluteDataPath);
    }

    internal (string? sync, string? local, string? value, LocalDataChangesType changesType)? InternalGetData(params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        return DBGetData(absoluteDataPath);
    }

    internal void InternalGetDataOrChildren(Action onEmpty, Action<(string? sync, string? local, string? value, LocalDataChangesType changesType)> onData, Action<(string key, LocalDataType type)[]> onPath, params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        DBGetDataOrRelativeChildren(onEmpty, onData, onPath, absoluteDataPath);
    }

    internal LocalDataType InternalGetDataType(params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        return DBGetDataType(absoluteDataPath);
    }

    internal void InternalGetDataOrRecursiveChildren(Action onEmpty, Action<(string? sync, string? local, string? value, LocalDataChangesType changesType)> onData, Action<string[][]> onPath, params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        DBGetDataOrRecursiveChildren(onEmpty, onData, onPath, absoluteDataPath);
    }

    internal void InternalGetDataOrRecursiveRelativeChildren(Action onEmpty, Action<(string? sync, string? local, string? value, LocalDataChangesType changesType)> onData, Action<string[][]> onPath, params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        DBGetDataOrRecursiveRelativeChildren(onEmpty, onData, onPath, absoluteDataPath);
    }

    internal string[][] InternalGetRecursiveRelativeChildren(params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        return DBGetRecursiveRelativeChildren(absoluteDataPath);
    }

    internal (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] InternalGetRecursiveData(params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        return DBGetRecursiveData(absoluteDataPath);
    }

    internal (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] InternalGetRecursiveRelativeData(params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        return DBGetRecursiveRelativeData(absoluteDataPath);
    }

    #endregion

    #region Sync Helpers

    private protected bool MakeChanges(string? newLocal, params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        bool hasChanges = false;

        var data = DBGetRecursiveRelativeData(absoluteDataPath);

        if (data.Length == 0)
        {
            if (newLocal != null)
            {
                DBSetData(null, newLocal, LocalDataChangesType.Create, absoluteDataPath);

                hasChanges = true;
            }

            DBPut(newLocal, absoluteDataPath);
        }
        else if (data.Length == 1 && data[0].path.Length == 0)
        {
            if (data[0].value != newLocal)
            {
                if (data[0].sync == null && newLocal == null)
                {
                    DBDeleteData(absoluteDataPath);
                    DBCancelPut(absoluteDataPath);
                }
                else if (data[0].sync == null && newLocal != null)
                {
                    DBSetData(data[0].sync, newLocal, LocalDataChangesType.Create, absoluteDataPath);
                    DBPut(newLocal, absoluteDataPath);
                }
                else if (data[0].sync != null && newLocal == null)
                {
                    DBSetData(data[0].sync, newLocal, LocalDataChangesType.Delete, absoluteDataPath);
                    DBPut(newLocal, absoluteDataPath);
                }
                else
                {
                    DBSetData(data[0].sync, newLocal, LocalDataChangesType.Update, absoluteDataPath);
                    DBPut(newLocal, absoluteDataPath);
                }

                hasChanges = true;
            }
        }
        else
        {
            if (newLocal != null)
            {
                DBSetData(null, newLocal, LocalDataChangesType.Create, absoluteDataPath);
                DBPut(newLocal, absoluteDataPath);
            }
            else if (data.All(i => i.changesType == LocalDataChangesType.Create))
            {
                DBDeleteData(absoluteDataPath);
                DBPut(null, absoluteDataPath);
            }
            else
            {
                DBSetData(null, null, LocalDataChangesType.Delete, absoluteDataPath);
                DBPut(null, absoluteDataPath);
            }

            hasChanges = true;
        }

        return hasChanges;
    }

    private protected bool MakeSync(string? newSync, params string[] path)
    {
        string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

        bool hasChanges = false;

        var data = DBGetRecursiveData(absoluteDataPath);

        if (data.Length == 0)
        {
            if (newSync != null)
            {
                DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);

                hasChanges = true;
            }
            else
            {
                DBGetNearestHierarchyDataOrRelativePath(
                    () =>
                    {

                    },
                    hierData =>
                    {
                        if (hierData.changesType == LocalDataChangesType.Create)
                        {
                            DBPut(hierData.local, hierData.path);
                        }
                        else
                        {
                            DBDeleteData(absoluteDataPath);
                            hasChanges = true;
                        }
                    },
                    hierPath =>
                    {
                        if (DBGetRelativeTypedChildren(hierPath).Length == 0)
                        {
                            DBDeleteData(hierPath);
                            hasChanges = true;
                        }
                    },
                    absoluteDataPath);
            }
        }
        else if (data.Length == 1 && data[0].path.Length == 0)
        {
            hasChanges = MakeSyncSingle(newSync, data[0].sync, data[0].local, data[0].value, data[0].changesType, absoluteDataPath);
        }
        else
        {
            if (newSync != null)
            {
                DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);

                hasChanges = true;
            }
            else
            {
                if (data.All(i => i.changesType == LocalDataChangesType.Synced))
                {
                    DBDeleteData(absoluteDataPath);
                }
                else
                {
                    foreach (var child in data)
                    {
                        switch (child.changesType)
                        {
                            case LocalDataChangesType.Create:
                                DBPut(child.local, child.path);
                                break;
                            case LocalDataChangesType.Update:
                                DBPut(child.local, child.path);
                                break;
                            case LocalDataChangesType.Delete:
                                DBPut(null, child.path);
                                break;
                        }
                    }
                }
            }
        }

        return hasChanges;
    }

    private protected bool MakeSync(IDictionary<string[], string?> values, params string[] path)
    {
        string[] absolutePath = DBGetAbsoluteDataPath(path);

        var data = DBGetRecursiveRelativeData(absolutePath);

        if (data.Length > 0 && data[0].path?.Length != 0)
        {
            foreach (var child in data)
            {
                if (!values.ContainsKey(child.path))
                {
                    string[] absoluteChildPath = DBGetAbsoluteDataPath(child.path);
                    if (child.changesType == LocalDataChangesType.Create)
                    {
                        DBPut(child.local, absoluteChildPath);
                    }
                    else
                    {
                        DBDeleteData(absoluteChildPath);
                    }
                }
            }
        }

        if (path.Length == 0)
        {
            foreach (KeyValuePair<string[], string?> pair in values)
            {
                MakeSync(pair.Value, pair.Key);
            }
        }
        else
        {
            foreach (KeyValuePair<string[], string?> pair in values)
            {
                if (pair.Key.Length == 0)
                {
                    MakeSync(pair.Value, path);
                }
                else if (pair.Key.Length == 1)
                {
                    string[] childPath = new string[path.Length + 1];
                    Array.Copy(path, 0, childPath, 0, path.Length);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    childPath[^1] = pair.Key[0];
#else
                    childPath[^1] = pair.Key[0];
#endif
                    MakeSync(pair.Value, childPath);
                }
                else if (pair.Key.Length > 1)
                {
                    string[] childPath = new string[path.Length + pair.Key.Length];
                    Array.Copy(path, 0, childPath, 0, path.Length);
                    Array.Copy(pair.Key, 0, childPath, path.Length, pair.Key.Length);
                    MakeSync(pair.Value, childPath);
                }
            }
        }
        return false;
    }

    private bool MakeSyncSingle(string? newSync, string? oldSync, string? local, string? value, LocalDataChangesType changesType, params string[] absoluteDataPath)
    {
        switch (changesType)
        {
            case LocalDataChangesType.Create:
                if (newSync == null)
                {
                    DBPut(local, absoluteDataPath);
                }
                else
                {
                    DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                }
                break;
            case LocalDataChangesType.Update:
                if (newSync == null)
                {
                    DBDeleteData(absoluteDataPath);
                }
                else if (newSync == oldSync)
                {
                    DBPut(local, absoluteDataPath);
                }
                else
                {
                    DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                }
                break;
            case LocalDataChangesType.Delete:
                if (newSync == null)
                {
                    DBDeleteData(absoluteDataPath);
                }
                else if (newSync == oldSync)
                {
                    DBPut(null, absoluteDataPath);
                }
                else
                {
                    DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                }
                break;
            case LocalDataChangesType.Synced:
                if (newSync == null)
                {
                    DBDeleteData(absoluteDataPath);
                }
                else if (newSync != oldSync)
                {
                    DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                }
                break;
        }

        return value != newSync;
    }

    #endregion

    #region DB Sync Helpers

    private void DBCancelPut(string[] absolutePath)
    {
        if (Started)
        {
            App.Database.DBCancelPut(absolutePath);
        }
    }

    private void DBPut(string? blob, string[] absolutePath)
    {
        if (blob == null)
        {

        }
        if (Started)
        {
            App.Database.DBPut(blob, absolutePath, args =>
            {
                if (IsDisposed)
                {
                    return;
                }

                writeTaskErrorControl.ConcurrentTokenCount = 1;

                if (
                    args.err.Exception is DatabaseUnauthorizedException ||
                    args.err.Exception is DatabasePaymentRequiredException)
                {
                    var data = DBGetData(args.writeTask.Path);
                    if (data.HasValue)
                    {
                        if (data.Value.sync == null)
                        {
                            DBDeleteData(args.writeTask.Path);
                        }
                        else
                        {
                            DBSetData(null, data.Value.local, data.Value.changesType, args.writeTask.Path);
                        }
                    }
                }
                else if (
                    args.err.Exception is OfflineModeException ||
                    args.err.Exception is OperationCanceledException ||
                    args.err.Exception is AuthException ||
                    args.err.Exception is DatabaseUndefinedException)
                {
                    args.err.Retry = writeTaskErrorControl.SendAsync(async delegate
                    {
                        await Task.Delay(App.Config.CachedDatabaseRetryDelay).ConfigureAwait(false);
                        return true;
                    });
                }

                OnError(args.writeTask.Uri, args.err.Exception);
            }, delegate
            {
                writeTaskErrorControl.ConcurrentTokenCount = App.Config.CachedDatabaseMaxConcurrentSyncWrites;
            });
        }
    }

    #endregion

    #region DB Local Helpers

    private void DBDeleteData(string[] absolutePath)
    {
        App.LocalDatabase.InternalDelete(LocalDatabase, absolutePath);
    }

    private string[] DBGetAbsoluteDataPath(string[]? path)
    {
        int pathLength = path?.Length ?? 0;
        string[] absoluteDataPath = new string[DBPath.Length + pathLength + 1];
        absoluteDataPath[0] = DatabaseApp.OfflineDatabaseIndicator;
        Array.Copy(DBPath, 0, absoluteDataPath, 1, DBPath.Length);
        if (path != null && pathLength > 0)
        {
            Array.Copy(path, 0, absoluteDataPath, DBPath.Length + 1, pathLength);
        }

        return absoluteDataPath;
    }

    private (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] DBGetRecursiveData(string[] absolutePath)
    {
        string? data = null;
        (string[] path, string value)[]? children = null;

        App.LocalDatabase.InternalTryGetValueOrRecursiveValues(LocalDatabase,
            v => data = v,
            c => children = c,
            absolutePath);

        if (data != null && !string.IsNullOrEmpty(data))
        {
            var deserialized = DeserializeData(data);
            if (deserialized.HasValue)
            {
                return new (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[]
                {
                    (Array.Empty<string>(), deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType)
                };
            }
        }
        else if (children != null && children.Length > 0)
        {
            (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] values = new (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[children.Length];

            for (int i = 0; i < values.Length; i++)
            {
                var deserialized = DeserializeData(children[i].value);
                if (deserialized.HasValue)
                {
                    values[i] = (children[i].path, deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType);
                }
            }

            return values;
        }

        return Array.Empty<(string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)>();
    }

    private (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] DBGetRecursiveRelativeData(string[] absolutePath)
    {
        string? data = null;
        (string[] path, string value)[]? children = null;

        App.LocalDatabase.InternalTryGetValueOrRecursiveRelativeValues(LocalDatabase,
           v => data = v,
           c => children = c,
           absolutePath);

        if (data != null && !string.IsNullOrEmpty(data))
        {
            var deserialized = DeserializeData(data);
            if (deserialized.HasValue)
            {
                return new (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[]
                {
                    (Array.Empty<string>(), deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType)
                };
            }
        }
        else if (children != null && children.Length > 0)
        {
            (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[] values = new (string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)[children.Length];

            for (int i = 0; i < values.Length; i++)
            {
                var deserialized = DeserializeData(children[i].value);
                if (deserialized.HasValue)
                {
                    values[i] = (children[i].path, deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType);
                }
            }

            return values;
        }

        return Array.Empty<(string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)>();
    }

    private (string? sync, string? local, string? value, LocalDataChangesType changesType)? DBGetData(string[] absolutePath)
    {
        string? data = App.LocalDatabase.InternalGetValue(LocalDatabase, absolutePath);

        if (data == null || string.IsNullOrEmpty(data))
        {
            return default;
        }

        var deserialized = DeserializeData(data);
        if (deserialized.HasValue)
        {
            return (deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType);
        }

        return default;
    }

    private void DBGetDataOrRelativeChildren(Action onEmpty, Action<(string? sync, string? local, string? value, LocalDataChangesType changesType)> onData, Action<(string key, LocalDataType type)[]> onPath, string[] absolutePath)
    {
        string? data = null;

        (string key, LocalDataType type)[]? children = null;

        App.LocalDatabase.InternalTryGetValueOrRelativeTypedChildren(LocalDatabase,
            v => data = v,
            c => children = c,
            absolutePath);

        if (data != null && !string.IsNullOrEmpty(data))
        {
            var deserialized = DeserializeData(data);
            if (deserialized.HasValue)
            {
                onData?.Invoke((deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType));

                return;
            }
        }

        if (children != null && children.Length > 0)
        {
            onPath?.Invoke(children);

            return;
        }

        onEmpty?.Invoke();
    }

    private void DBGetDataOrRecursiveChildren(Action onEmpty, Action<(string? sync, string? local, string? value, LocalDataChangesType changesType)> onData, Action<string[][]> onPath, string[] absolutePath)
    {
        string? data = null;

        string[][]? children = null;

        App.LocalDatabase.InternalTryGetValueOrRecursiveChildren(LocalDatabase,
            v => data = v,
            c => children = c,
            absolutePath);

        if (data != null && !string.IsNullOrEmpty(data))
        {
            var deserialized = DeserializeData(data);
            if (deserialized.HasValue)
            {
                onData?.Invoke((deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType));

                return;
            }
        }

        if (children != null && children.Length > 0)
        {
            onPath?.Invoke(children);

            return;
        }

        onEmpty?.Invoke();
    }

    private void DBGetDataOrRecursiveRelativeChildren(Action onEmpty, Action<(string? sync, string? local, string? value, LocalDataChangesType changesType)> onData, Action<string[][]> onPath, string[] absolutePath)
    {
        string? data = null;

        string[][]? children = null;

        App.LocalDatabase.InternalTryGetValueOrRecursiveRelativeChildren(LocalDatabase,
            v => data = v,
            c => children = c,
            absolutePath);

        if (data != null && !string.IsNullOrEmpty(data))
        {
            var deserialized = DeserializeData(data);
            if (deserialized.HasValue)
            {
                onData?.Invoke((deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType));

                return;
            }
        }

        if (children != null && children.Length > 0)
        {
            onPath?.Invoke(children);

            return;
        }

        onEmpty?.Invoke();
    }

    private LocalDataType DBGetDataType(string[] absolutePath)
    {
        return App.LocalDatabase.InternalGetDataType(LocalDatabase, absolutePath);
    }

    private void DBGetNearestHierarchyDataOrRelativePath(Action onEmpty, Action<(string[] path, string? sync, string? local, string? value, LocalDataChangesType changesType)> onData, Action<string[]> onPath, string[] absolutePath)
    {
        (string[] path, string? value)? hierData = null;

        string[]? hierPath = null;

        App.LocalDatabase.InternalTryGetNearestHierarchyValueOrPath(LocalDatabase,
            d => hierData = d,
            p => hierPath = p,
            absolutePath);

        if (hierData != null && hierData.Value.value != null)
        {
            var deserialized = DeserializeData(hierData.Value.value);
            if (deserialized.HasValue)
            {
                onData?.Invoke((hierData.Value.path, deserialized.Value.sync, deserialized.Value.local, deserialized.Value.value, deserialized.Value.changesType));

                return;
            }
        }

        if (hierPath != null && hierPath.Length > 0)
        {
            onPath?.Invoke(hierPath);

            return;
        }

        onEmpty?.Invoke();
    }

    private (string key, LocalDataType type)[] DBGetRelativeTypedChildren(string[] absolutePath)
    {
        return App.LocalDatabase.InternalGetRelativeTypedChildren(LocalDatabase, absolutePath);
    }

    private string[][] DBGetRecursiveRelativeChildren(string[] absolutePath)
    {
        return App.LocalDatabase.InternalGetRecursiveRelativeChildren(LocalDatabase, absolutePath);
    }

    private void DBSetData(string? sync, string? local, LocalDataChangesType changesType, string[] absolutePath)
    {
        string serialized = SerializeData(sync, local, changesType);

        App.LocalDatabase.InternalSetValue(LocalDatabase, serialized, absolutePath);
    }

    private static (string? sync, string? local, string? value, LocalDataChangesType changesType)? DeserializeData(string data)
    {
        string?[]? deserialized = StringSerializer.Deserialize(data);
        if (deserialized != null && deserialized.Length == 3)
        {
            string? sync = deserialized[0];
            string? local = deserialized[1];
            return deserialized[2] switch
            {
                "c" => (sync, local, local ?? sync, LocalDataChangesType.Create),
                "u" => (sync, local, local ?? sync, LocalDataChangesType.Update),
                "d" => (sync, local, null, LocalDataChangesType.Delete),
                _ => (sync, local, local ?? sync, LocalDataChangesType.Synced),
            };
        }

        return default;
    }

    private static string SerializeData(string? sync, string? local, LocalDataChangesType changesType)
    {
        string?[] data = new string[3];

        data[0] = sync;
        data[1] = local;
        data[2] = changesType switch
        {
            LocalDataChangesType.Create => "c",
            LocalDataChangesType.Update => "u",
            LocalDataChangesType.Delete => "d",
            LocalDataChangesType.Synced => null,
            _ => null,
        };
        return StringSerializer.Serialize(data);
    }

    #endregion

    #region Object Members

    /// <inheritdoc/>
    public override string ToString()
    {
        return AbsoluteUrl;
    }

    #endregion

    #region Disposable Members

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            App.LocalDatabase.InternalUnsubscribe(LocalDatabase, OnDataChanges);
            if (Root != null)
            {
                Root.Error -= Parent_Error;
            }
            if (Parent != null)
            {
                Parent.Disposing -= Parent_Disposing;
            }
            DataChanges = null;
            ImmediateDataChanges = null;
            Error = null;
        }
        base.Dispose(disposing);
    }

    #endregion

    #region INullableObject Members

    /// <inheritdoc/>
    public bool SetNull()
    {
        return SetValue(null);
    }

    /// <inheritdoc/>
    public bool IsNull()
    {
        return IsNull(Array.Empty<string>());
    }

    #endregion

    #region IClonable Members

    object ICloneable.Clone() => Clone();

    #endregion
}
