using Newtonsoft.Json;
using ObservableHelpers;
using ObservableHelpers.Abstraction;
using ObservableHelpers.Utilities;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Realtime
{
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
        public RealtimeWire Root { get; }

        /// <summary>
        /// The parent <see cref="RealtimeInstance"/> of the instance.
        /// </summary>
        public RealtimeInstance Parent { get; }

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
        /// Gets or sets the firebase realtime database max concurrent writes.
        /// </summary>
        public int MaxConcurrentWrites
        {
            get => writeTaskPutControl.ConcurrentTokenCount;
            set => writeTaskPutControl.ConcurrentTokenCount = value;
        }

        /// <summary>
        /// Event raised on the current context when there is data changes on the node or sub nodes.
        /// </summary>
        public event EventHandler<DataChangesEventArgs> DataChanges;

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
        public event EventHandler<WireExceptionEventArgs> Error;

        private string absoluteUrl;
        private string[] absolutePath;

        private readonly OperationInvoker writeTaskPutControl = new OperationInvoker(10);
        private readonly OperationInvoker writeTaskErrorControl = new OperationInvoker(0);
        private readonly ConcurrentDictionary<string[], WriteTask> writeTasks = new ConcurrentDictionary<string[], WriteTask>(PathEqualityComparer.Instance);
        private readonly RWLockDictionary<string[]> rwLock = new RWLockDictionary<string[]>(LockRecursionPolicy.SupportsRecursion, PathEqualityComparer.Instance);

        #endregion

        #region Initializers

        private protected RealtimeInstance(RestfulFirebaseApp app, IFirebaseQuery query, ILocalDatabase localDatabase)
        {
            App = app;
            Query = query;
            LocalDatabase = localDatabase;

            ReloadQueryUrlValues();

            App.LocalDatabase.InternalSubscribe(LocalDatabase, OnDataChanges);
        }

        private protected RealtimeInstance(RestfulFirebaseApp app, RealtimeWire root, RealtimeInstance parent, IFirebaseQuery query, ILocalDatabase localDatabase)
           : this(app, query, localDatabase)
        {
            Root = root;
            Parent = parent;

            Root.Error += Parent_Error;
            Parent.Disposing += Parent_Disposing;
        }

        private protected RealtimeInstance(RestfulFirebaseApp app, RealtimeWire root, RealtimeInstance parent, string path, ILocalDatabase localDatabase)
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
        /// <exception cref="DatabaseRealtimeWireNotStarted">
        /// Throws when wire was not started.
        /// </exception>
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
        /// <exception cref="DatabaseRealtimeWireNotStarted">
        /// Throws when wire was not started.
        /// </exception>
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
        /// Specify <c>true</c> whether the task will be cancelled on error; otherwise <c>false</c>.
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
        /// <exception cref="DatabaseRealtimeWireNotStarted">
        /// Throws when wire was not started.
        /// </exception>
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
        /// Specify <c>true</c> whether the task will be cancelled on error; otherwise <c>false</c>.
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
        /// <exception cref="DatabaseRealtimeWireNotStarted">
        /// Throws when wire was not started.
        /// </exception>
        /// <exception cref="DatabaseForbiddenNodeNameCharacter">
        /// Throws when any node has forbidden node name character.
        /// </exception>
        public async Task<bool> WaitForSynced(bool cancelOnError = true, CancellationToken? cancellationToken = null, params string[] path)
        {
            if (IsDisposed)
            {
                return false;
            }

            EnsureValidPath(path);

            if (!Started)
            {
                throw new DatabaseRealtimeWireNotStarted(nameof(WaitForSynced));
            }

            bool cancel = false;
            void RealtimeInstance2_Error(object sender, WireExceptionEventArgs e)
            {
                cancel = true;
            }
            if (cancelOnError)
            {
                Error += RealtimeInstance2_Error;
            }
            async Task<bool> waitTask()
            {
                while (!IsSynced(path) && !App.Config.OfflineMode && !cancel && !(cancellationToken?.IsCancellationRequested ?? false))
                {
                    try
                    {
                        if (cancellationToken.HasValue)
                        {
                            await Task.Delay(App.Config.DatabaseRetryDelay, cancellationToken.Value).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Delay(App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                        }
                    }
                    catch { }
                }
                return IsSynced(path);
            }
            bool result = await Task.Run(waitTask).ConfigureAwait(false);
            if (cancelOnError)
            {
                Error -= RealtimeInstance2_Error;
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
        public (int total, int synced) GetDataCount(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidPath(path);

            int total = 0;
            int syned = 0;
            (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[] children = InternalGetRecursiveData(path);
            foreach ((string[] childPath, string sync, string local, string value, LocalDataChangesType changesType) in children)
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
        /// Gets <c>true</c> whether the node is fully synced; otherwise <c>false</c>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to check.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the node is fully synced; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="DatabaseForbiddenNodeNameCharacter">
        /// Throws when any node has forbidden node name character.
        /// </exception>
        public bool IsSynced(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidPath(path);

            if (HasFirstStream)
            {
                (int total, int synced) = GetDataCount(path);
                return total == synced;
            }

            return false;
        }

        /// <summary>
        /// Gets <c>true</c> whether the node is locally available; otherwise <c>false</c>.
        /// </summary>
        /// <param name="path">
        /// The path of the data to check.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the node is locally available; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="DatabaseForbiddenNodeNameCharacter">
        /// Throws when any node has forbidden node name character.
        /// </exception>
        public bool IsLocallyAvailable(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidPath(path);

            (int total, int synced) = GetDataCount(path);
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
        public T PutModel<T>(T model, params string[] path)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
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

            return model;
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
        /// A <see cref="Task"/> that represents the provided <paramref name="model"/>.
        /// </returns>
        /// <exception cref="DatabaseInvalidModel">
        /// Throws when <paramref name="model"/> is not a valid model.
        /// </exception>
        /// <exception cref="DatabaseForbiddenNodeNameCharacter">
        /// Throws when any node has forbidden node name character.
        /// </exception>
        public async Task<T> PutModelAsync<T>(T model, params string[] path)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            if (model is IInternalRealtimeModel internalModel)
            {
                if (path == null || path.Length == 0)
                {
                    await internalModel.AttachRealtimeAsync(this, true);
                }
                else
                {
                    EnsureValidPath(path);
                    await internalModel.AttachRealtimeAsync(Child(path), true);
                }
            }
            else
            {
                throw new DatabaseInvalidModel();
            }

            return model;
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
        public T SubModel<T>(T model, params string[] path)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
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

            return model;
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
        /// A <see cref="Task"/> that represents the provided <paramref name="model"/>.
        /// </returns>
        /// <exception cref="DatabaseInvalidModel">
        /// Throws when <paramref name="model"/> is not a valid model.
        /// </exception>
        /// <exception cref="DatabaseForbiddenNodeNameCharacter">
        /// Throws when any node has forbidden node name character.
        /// </exception>
        public async Task<T> SubModelAsync<T>(T model, params string[] path)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            if (model is IInternalRealtimeModel internalModel)
            {
                if (path == null || path.Length == 0)
                {
                    await internalModel.AttachRealtimeAsync(this, false);
                }
                else
                {
                    EnsureValidPath(path);
                    await internalModel.AttachRealtimeAsync(Child(path), false);
                }
            }
            else
            {
                throw new DatabaseInvalidModel();
            }

            return model;
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

        private void OnPutError(WriteTask writeTask, RetryExceptionEventArgs err)
        {
            if (IsDisposed)
            {
                return;
            }

            if (err.Exception is DatabaseUnauthorizedException ex)
            {
                (string sync, string local, string value, LocalDataChangesType changesType) = DBGetData(writeTask.Path);
                if (sync == null)
                {
                    DBDeleteData(writeTask.Path);
                }
                else
                {
                    DBSetData(null, local, changesType, writeTask.Path);
                }
            }

            OnError(writeTask.Uri, err.Exception);
        }

        private void Parent_Error(object sender, WireExceptionEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            string baseUri = absoluteUrl.Trim('/');
            if (e.Uri.StartsWith(baseUri))
            {
                SelfError(e);
            }
        }

        private void Parent_Disposing(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose();
        }

        private void OnDataChanges(object sender, DataChangesEventArgs args)
        {
            if (absolutePath.Length > args.Path.Length - 1)
            {
                return;
            }
            for (int i = 0; i < absolutePath.Length; i++)
            {
                if (absolutePath[i] != args.Path[i + 1])
                {
                    return;
                }
            }

            string[] path = new string[args.Path.Length - 1 - absolutePath.Length];
            if (path.Length != 0)
            {
                Array.Copy(args.Path, args.Path.Length - path.Length, path, 0, path.Length);
            }

            ContextSend(delegate
            {
                DataChanges?.Invoke(this, new DataChangesEventArgs(path));
            });
        }

        private void SelfError(WireExceptionEventArgs e)
        {
            ContextSend(delegate
            {
                Error?.Invoke(this, e);
            });
        }

        private void ReloadQueryUrlValues()
        {
            absoluteUrl = Query.GetAbsoluteUrl();
            absolutePath = UrlUtilities.Separate(absoluteUrl.Substring(8)); // Removes 'https://'
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
        public RealtimeInstance Child(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidAndNonEmptyPath(path);

            RealtimeInstance childWire = null;
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
        public virtual RealtimeInstance Clone()
        {
            if (IsDisposed)
            {
                return default;
            }

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
        public bool HasChildren(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

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
        public (string key, LocalDataType type)[] GetChildren(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

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
        public (string sync, string local, string value, LocalDataChangesType changesType) GetData(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

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
        public string[][] GetRecursiveChildren(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidPath(path);

            return InternalGetRecursiveChildren(path);
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
        public (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[] GetRecursiveData(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidPath(path);

            return InternalGetRecursiveData(path);
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
        public string GetValue(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidPath(path);

            return InternalGetData(path).value;
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
        public bool IsNull(params string[] path)
        {
            if (IsDisposed)
            {
                return true;
            }

            EnsureValidPath(path);

            bool isNull = true;

            InternalGetDataOrRecursiveChildren(
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
        public bool SetValue(string value, params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            EnsureValidPath(path);

            return MakeChanges(value, path);
        }

        private void EnsureValidPath(string[] path)
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

        private void EnsureValidAndNonEmptyPath(string[] path)
        {
            if (path == null || path.Length == 0)
            {
                throw StringNullOrEmptyException.FromSingleArgument(nameof(path));
            }
            EnsureValidPath(path);
        }

        #endregion

        #region Sync Helpers

        private protected bool MakeChanges(string newLocal, params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return rwLock.LockWrite(absoluteDataPath, () =>
            {
                DBCancelPut(absoluteDataPath);

                bool hasChanges = false;

                DBGetDataOrRecursiveChildren(
                    delegate
                    {
                        if (newLocal != null)
                        {
                            DBSetData(null, newLocal, LocalDataChangesType.Create, absoluteDataPath);
                            DBPut(newLocal, absoluteDataPath);

                            hasChanges = true;
                        }
                    },
                    data =>
                    {
                        if (data.value != newLocal)
                        {
                            if (data.sync == null && newLocal == null)
                            {
                                DBDeleteData(absoluteDataPath);
                            }
                            else if (data.sync == null && newLocal != null)
                            {
                                DBSetData(data.sync, newLocal, LocalDataChangesType.Create, absoluteDataPath);
                                DBPut(newLocal, absoluteDataPath);
                            }
                            else if (data.sync != null && newLocal == null)
                            {
                                DBSetData(data.sync, newLocal, LocalDataChangesType.Delete, absoluteDataPath);
                                DBPut(newLocal, absoluteDataPath);
                            }
                            else
                            {
                                DBSetData(data.sync, newLocal, LocalDataChangesType.Update, absoluteDataPath);
                                DBPut(newLocal, absoluteDataPath);
                            }

                            hasChanges = true;
                        }
                    },
                    children =>
                    {
                        if (newLocal != null)
                        {
                            DBSetData(null, newLocal, LocalDataChangesType.Create, absoluteDataPath);
                            DBPut(newLocal, absoluteDataPath);
                        }
                        else
                        {
                            DBSetData(null, null, LocalDataChangesType.Delete, absoluteDataPath);
                            DBPut(null, absoluteDataPath);
                        }

                        hasChanges = true;
                    },
                    absoluteDataPath);

                return hasChanges;
            });
        }

        private protected bool MakeSync(string newSync, params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return rwLock.LockWrite(absoluteDataPath, () =>
            {
                DBCancelPut(absoluteDataPath);

                bool hasChanges = false;

                DBGetDataOrRecursiveChildren(
                    delegate
                    {
                        if (newSync != null)
                        {
                            DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);

                            hasChanges = true;
                        }
                    },
                    data =>
                    {
                        hasChanges = MakeSyncSingle(newSync, data.sync, data.local, data.value, data.changesType, absoluteDataPath);
                    },
                    children =>
                    {
                        if (newSync != null)
                        {
                            DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);

                            hasChanges = true;
                        }
                        else
                        {
                            foreach (string[] child in children)
                            {
                                (string oldSync, string local, string value, LocalDataChangesType changesType) = DBGetData(absoluteDataPath);
                                if (changesType != LocalDataChangesType.Synced)
                                {
                                    DBPut(local, absoluteDataPath);
                                }
                            }
                        }

                    },
                    absoluteDataPath);

                return hasChanges;
            });
        }

        private protected bool MakeSync(IDictionary<string[], string> values, params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return rwLock.LockWrite(absoluteDataPath, () =>
            {
                string[][] children = InternalGetRecursiveChildren(path);
                foreach (string[] child in children)
                {
                    if (!values.ContainsKey(child))
                    {
                        (string oldSync, string local, string value, LocalDataChangesType changesType) = DBGetData(absoluteDataPath);
                        string[] absoluteChildPath = DBGetAbsoluteDataPath(child);
                        MakeSyncSingle(null, oldSync, local, value, changesType, absoluteChildPath);
                    }
                }

                if (path.Length == 0)
                {
                    foreach (KeyValuePair<string[], string> pair in values)
                    {
                        MakeSync(pair.Value, pair.Key);
                    }
                }
                else
                {
                    foreach (KeyValuePair<string[], string> pair in values)
                    {
                        if (pair.Key.Length == 0)
                        {
                            MakeSync(pair.Value, path);
                        }
                        else if (pair.Key.Length == 1)
                        {
                            string[] childPath = new string[path.Length + 1];
                            Array.Copy(path, 0, childPath, 0, path.Length);
                            childPath[childPath.Length - 1] = pair.Key[0];
                            MakeSync(pair.Value, childPath);
                        }
                        else if (pair.Key.Length > 1)
                        {
                            string[] childPath = new string[path.Length + pair.Key.Length];
                            Array.Copy(path, 0, childPath, 0, path.Length);
                            Array.Copy(pair.Key, 0, childPath, path.Length - 1, pair.Key.Length);
                            MakeSync(pair.Value, childPath);
                        }
                    }
                }
                return false;
            });
        }

        private bool MakeSyncSingle(string newSync, string oldSync, string local, string value, LocalDataChangesType changesType, params string[] absoluteDataPath)
        {
            return rwLock.LockWrite(absoluteDataPath, () =>
            {
                if (local == null)
                {
                    if (newSync == null)
                    {
                        DBDeleteData(absoluteDataPath);
                    }
                    else
                    {
                        DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                    }
                }
                else if (local == newSync)
                {
                    DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                }
                else
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
                            else if (oldSync == newSync)
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
                            else if (oldSync == newSync)
                            {
                                DBPut(null, absoluteDataPath);
                            }
                            else
                            {
                                DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                            }
                            break;
                        case LocalDataChangesType.Synced:
                            DBSetData(newSync, null, LocalDataChangesType.Synced, absoluteDataPath);
                            break;
                    }
                }

                return value != newSync;
            });
        }

        #endregion

        #region Internal Helpers

        private protected (string key, LocalDataType type)[] InternalGetChildren(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return DBGetChildren(absoluteDataPath);
        }

        private protected (string sync, string local, string value, LocalDataChangesType changesType) InternalGetData(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return DBGetData(absoluteDataPath);
        }

        private protected LocalDataType InternalGetDataType(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return DBGetDataType(absoluteDataPath);
        }

        private protected void InternalGetDataOrRecursiveChildren(Action onEmpty, Action<(string sync, string local, string value, LocalDataChangesType changesType)> onData, Action<string[][]> onPath, params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            DBGetDataOrRecursiveChildren(onEmpty, onData, onPath, absoluteDataPath);
        }

        private protected string[][] InternalGetRecursiveChildren(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return DBGetRecursiveChildren(absoluteDataPath);
        }

        private protected (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[] InternalGetRecursiveData(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return DBGetRecursiveData(absoluteDataPath);
        }

        #endregion

        #region DB Sync Helpers

        private void DBCancelPut(string[] path)
        {
            if (writeTasks.TryRemove(path, out WriteTask writeTask))
            {
                writeTask.Cancel();
            }
        }

        private bool DBIsWriting(string[] path)
        {
            if (writeTasks.TryGetValue(path, out WriteTask writeTask))
            {
                return !writeTask.IsCancelled;
            }
            else
            {
                return false;
            }
        }

        private void DBPut(string blob, string[] path)
        {
            if (writeTasks.TryGetValue(path, out WriteTask writeTask))
            {
                if (writeTask.Blob != blob)
                {
                    writeTask.ReWriteRequested = true;
                    writeTask.Blob = blob;
                }
            }
            else
            {
                writeTask = new WriteTask(this, path, blob,
                    (s, e) => writeTasks.TryRemove(path, out _),
                    (s, e) => OnPutError(writeTask, e));
                writeTasks.TryAdd(path, writeTask);
                writeTask.Run();
            }
        }

        #endregion

        #region DB Local Helpers

        private void DBDeleteData(string[] path)
        {
            rwLock.LockWrite(path, () => App.LocalDatabase.InternalDelete(LocalDatabase, path));
        }

        private string[] DBGetAbsoluteDataPath(string[] path)
        {
            int pathLength = path?.Length ?? 0;
            string[] absoluteDataPath = new string[absolutePath.Length + pathLength + 1];
            absoluteDataPath[0] = DatabaseApp.OfflineDatabaseIndicator;
            Array.Copy(absolutePath, 0, absoluteDataPath, 1, absolutePath.Length);
            if (pathLength > 0)
            {
                Array.Copy(path, 0, absoluteDataPath, absolutePath.Length + 1, pathLength);
            }

            return absoluteDataPath;
        }

        private (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[] DBGetRecursiveData(string[] path)
        {
            string data = null;
            (string[] path, string value)[] children = null;

            rwLock.LockWrite(path, () => App.LocalDatabase.InternalTryGetValueOrRecursiveRelativeValues(LocalDatabase,
                v => data = v,
                c => children = c,
                path));

            if (!string.IsNullOrEmpty(data))
            {
                string[] deserialized = StringUtilities.Deserialize(data);
                if (deserialized != null && deserialized.Length == 3)
                {
                    string sync = deserialized[0];
                    string local = deserialized[1];
                    string value = local == null ? sync : local;
                    LocalDataChangesType changesType = LocalDataChangesType.Synced;
                    switch (deserialized[2])
                    {
                        case "c":
                            changesType = LocalDataChangesType.Create;
                            break;
                        case "d":
                            changesType = LocalDataChangesType.Delete;
                            break;
                        case "u":
                            changesType = LocalDataChangesType.Update;
                            break;
                    }

                    return new (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[] { (new string[0], sync, local, value, changesType) };
                }
            }
            else if (children != null && children.Length > 0)
            {
                (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[] values = new (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[children.Length];
                
                for (int i = 0; i < values.Length; i++)
                {
                    string[] deserialized = StringUtilities.Deserialize(children[i].value);
                    if (deserialized != null && deserialized.Length == 3)
                    {
                        string sync = deserialized[0];
                        string local = deserialized[1];
                        string value = local == null ? sync : local;
                        LocalDataChangesType changesType = LocalDataChangesType.Synced;
                        switch (deserialized[2])
                        {
                            case "c":
                                changesType = LocalDataChangesType.Create;
                                break;
                            case "d":
                                changesType = LocalDataChangesType.Delete;
                                break;
                            case "u":
                                changesType = LocalDataChangesType.Update;
                                break;
                        }

                        values[i] = (children[i].path, sync, local, value, changesType);
                    }
                }

                return values;
            }

            return new (string[] path, string sync, string local, string value, LocalDataChangesType changesType)[0];
        }

        private (string sync, string local, string value, LocalDataChangesType changesType) DBGetData(string[] path)
        {
            string data = rwLock.LockRead(path, () => App.LocalDatabase.InternalGetValue(LocalDatabase, path));

            if (string.IsNullOrEmpty(data))
            {
                return default;
            }

            string[] deserialized = StringUtilities.Deserialize(data);
            if (deserialized != null && deserialized.Length == 3)
            {
                string sync = deserialized[0];
                string local = deserialized[1];
                string value = local == null ? sync : local;
                LocalDataChangesType changesType = LocalDataChangesType.Synced;
                switch (deserialized[2])
                {
                    case "c":
                        changesType = LocalDataChangesType.Create;
                        break;
                    case "d":
                        changesType = LocalDataChangesType.Delete;
                        break;
                    case "u":
                        changesType = LocalDataChangesType.Update;
                        break;
                }

                return (sync, local, value, changesType);
            }

            return default;
        }

        private void DBGetDataOrRecursiveChildren(Action onEmpty, Action<(string sync, string local, string value, LocalDataChangesType changesType)> onData, Action<string[][]> onPath, string[] path)
        {
            string data = null;

            string[][] children = null;

            rwLock.LockWrite(path, () => App.LocalDatabase.InternalTryGetValueOrRecursiveRelativeChildren(LocalDatabase,
                v => data = v,
                c => children = c,
                path));

            if (!string.IsNullOrEmpty(data))
            {
                string[] deserialized = StringUtilities.Deserialize(data);
                if (deserialized != null && deserialized.Length == 3)
                {
                    string sync = deserialized[0];
                    string local = deserialized[1];
                    string value = local == null ? sync : local;
                    LocalDataChangesType changesType = LocalDataChangesType.Synced;
                    switch (deserialized[2])
                    {
                        case "c":
                            changesType = LocalDataChangesType.Create;
                            break;
                        case "d":
                            changesType = LocalDataChangesType.Delete;
                            break;
                        case "u":
                            changesType = LocalDataChangesType.Update;
                            break;
                    }

                    onData?.Invoke((sync, local, value, changesType));
                }
            }
            else if (children != null && children.Length > 0)
            {
                onPath?.Invoke(children);
            }
            else
            {
                onEmpty?.Invoke();
            }
        }

        private LocalDataType DBGetDataType(string[] path)
        {
            return rwLock.LockRead(path, () => App.LocalDatabase.InternalGetDataType(LocalDatabase, path));
        }

        private (string key, LocalDataType type)[] DBGetChildren(string[] path)
        {
            return rwLock.LockRead(path, () => App.LocalDatabase.InternalGetRelativeTypedChildren(LocalDatabase, path));
        }

        private string[][] DBGetRecursiveChildren(string[] path)
        {
            return rwLock.LockRead(path, () => App.LocalDatabase.InternalGetRecursiveRelativeChildren(LocalDatabase, path));
        }

        private void DBSetData(string sync, string local, LocalDataChangesType changesType, string[] path)
        {
            string[] data = new string[3];

            data[0] = sync;
            data[1] = local;
            switch (changesType)
            {
                case LocalDataChangesType.Create:
                    data[2] = "c";
                    break;
                case LocalDataChangesType.Delete:
                    data[2] = "d";
                    break;
                case LocalDataChangesType.Update:
                    data[2] = "u";
                    break;
                default:
                    data[2] = null;
                    break;
            }

            string serialized = StringUtilities.Serialize(data);

            rwLock.LockWrite(path, () => App.LocalDatabase.InternalSetValue(LocalDatabase, serialized, path));
        }

        #endregion

        #region Object Members

        /// <inheritdoc/>
        public override string ToString()
        {
            return absoluteUrl;
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
            return IsNull(null);
        }

        #endregion

        #region IClonable Members

        object ICloneable.Clone() => Clone();

        #endregion

        #region Helper Classes

        private class WriteTask
        {
            public RealtimeInstance RealtimeInstance { get; }

            public string[] Path { get; }

            public string Uri { get; }

            public string Blob { get; set; }

            public IFirebaseQuery Query { get; }

            public bool IsWritting { get; private set; }

            public bool IsCancelled => cancellationSource.IsCancellationRequested;

            public bool ReWriteRequested
            {
                get => reWriteRequested;
                set
                {
                    if (value)
                    {
                        cancellationSource = new CancellationTokenSource();
                    }
                    reWriteRequested = value;
                }
            }

            public CancellationToken CancellationToken => cancellationSource.Token;

            private CancellationTokenSource cancellationSource;

            private bool reWriteRequested;

            private readonly EventHandler<RetryExceptionEventArgs> error;
            private readonly EventHandler finish;

            public WriteTask(
                RealtimeInstance realtimeInstance,
                string[] path,
                string blob,
                EventHandler finish,
                EventHandler<RetryExceptionEventArgs> error)
            {
                RealtimeInstance = realtimeInstance;
                Path = path;
                Uri = "https://" + UrlUtilities.Combine(path.Skip(1));
                Blob = blob;
                this.finish = finish;
                this.error = error;
                Query = new ChildQuery(realtimeInstance.App, null, () => Uri);
                cancellationSource = new CancellationTokenSource();
            }

            public async void Run()
            {
                if (IsWritting || IsCancelled)
                {
                    finish?.Invoke(this, new EventArgs());
                    return;
                }
                IsWritting = true;

                do
                {
                    try
                    {
                        await RealtimeInstance.writeTaskPutControl.SendAsync(async delegate
                        {
                            if (IsCancelled)
                            {
                                return;
                            }
                            try
                            {
                                RealtimeInstance.writeTaskErrorControl.ConcurrentTokenCount = RealtimeInstance.MaxConcurrentWrites;
                                await Query.Put(() => Blob == null ? null : JsonConvert.SerializeObject(Blob), cancellationSource.Token, err =>
                                {
                                    if (IsCancelled)
                                    {
                                        return;
                                    }
                                    RealtimeInstance.writeTaskErrorControl.ConcurrentTokenCount = 1;
                                    Type exType = err.Exception.GetType();
                                    if (err.Exception is OfflineModeException)
                                    {
                                        err.Retry = RealtimeInstance.writeTaskErrorControl.SendAsync(async delegate
                                        {
                                            await Task.Delay(RealtimeInstance.App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                                            return true;
                                        });
                                    }
                                    else if (err.Exception is OperationCanceledException)
                                    {
                                        err.Retry = RealtimeInstance.writeTaskErrorControl.SendAsync(async delegate
                                        {
                                            await Task.Delay(RealtimeInstance.App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                                            return true;
                                        });
                                    }
                                    else if (err.Exception is AuthException)
                                    {
                                        err.Retry = RealtimeInstance.writeTaskErrorControl.SendAsync(async delegate
                                        {
                                            await Task.Delay(RealtimeInstance.App.Config.DatabaseRetryDelay).ConfigureAwait(false);
                                            return true;
                                        });
                                    }

                                    error?.Invoke(this, err);
                                }).ConfigureAwait(false);
                            }
                            catch { }
                            if (!IsCancelled)
                            {
                                RealtimeInstance.writeTaskErrorControl.ConcurrentTokenCount = RealtimeInstance.MaxConcurrentWrites;
                            }
                        }, cancellationSource.Token).ConfigureAwait(false);
                    }
                    catch { }
                }
                while (ReWriteRequested);

                IsWritting = false;

                finish?.Invoke(this, new EventArgs());
            }

            public void Cancel()
            {
                if (!IsCancelled)
                {
                    cancellationSource.Cancel();
                }
            }
        }

        #endregion
    }
}
