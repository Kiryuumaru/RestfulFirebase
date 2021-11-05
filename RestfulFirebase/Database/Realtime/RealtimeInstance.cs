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
        /// Gets or sets the firebase realtime database max concurrent writes.
        /// </summary>
        public int MaxConcurrentWrites
        {
            get => maxConcurrentWrites;
            set
            {
                writeTaskPutControl.ConcurrentTokenCount = value;
                maxConcurrentWrites = value;
            }
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
        /// <para><see cref="OperationCanceledException"/> - The operation was cancelled.</para>
        /// </remarks>
        public event EventHandler<WireExceptionEventArgs> Error;

        private int maxConcurrentWrites = 10;

        private string absoluteUrl;
        private string[] absolutePath;

        private OperationInvoker writeTaskPutControl;
        private OperationInvoker writeTaskErrorControl;
        private ConcurrentDictionary<string[], WriteTask> writeTasks;
        private RWLockDictionary<string[]> rwLock;

        #endregion

        #region Initializers

        private protected RealtimeInstance(RestfulFirebaseApp app, IFirebaseQuery query, ILocalDatabase localDatabase)
        {
            writeTaskPutControl = new OperationInvoker(0);
            writeTaskErrorControl = new OperationInvoker(0);
            writeTasks = new ConcurrentDictionary<string[], WriteTask>(PathEqualityComparer.Instance);
            rwLock = new RWLockDictionary<string[]>(LockRecursionPolicy.SupportsRecursion, PathEqualityComparer.Instance);

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
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public Task<bool> WaitForSynced(TimeSpan timeout)
        {
            return WaitForSynced(true, new CancellationTokenSource(timeout).Token);
        }

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete when the instance is fully synced.
        /// </summary>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> for the wait synced status.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public Task<bool> WaitForSynced(CancellationToken cancellationToken)
        {
            return WaitForSynced(true, cancellationToken);
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
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public Task<bool> WaitForSynced(bool cancelOnError, TimeSpan timeout)
        {
            return WaitForSynced(cancelOnError, new CancellationTokenSource(timeout).Token);
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
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public async Task<bool> WaitForSynced(bool cancelOnError = true, CancellationToken? cancellationToken = null)
        {
            if (IsDisposed)
            {
                return false;
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
                while (!IsSynced() && !cancel && !(cancellationToken?.IsCancellationRequested ?? false))
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
                return IsSynced();
            }
            bool result = await Task.Run(waitTask).ConfigureAwait(false);
            if (cancelOnError)
            {
                Error -= RealtimeInstance2_Error;
            }
            return result;
        }

        /// <summary>
        /// Gets the data cached of the instance.
        /// </summary>
        public (int total, int synced) GetDataCount()
        {
            int total = 0;
            int syned = 0;
            foreach (string[] path in InternalGetAllChildren())
            {
                (string sync, string local, string value, LocalDataChangesType changesType) = InternalGetData(path);
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
        public bool IsSynced()
        {
            (int total, int synced) = GetDataCount();
            return total == synced;
        }

        /// <summary>
        /// Gets <c>true</c> whether the node is locally available; otherwise <c>false</c>.
        /// </summary>
        public bool IsLocallyAvailable()
        {
            (int total, int synced) = GetDataCount();
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
        /// <returns>
        /// The provided <paramref name="model"/>.
        /// </returns>
        /// <exception cref="DatabaseInvalidModel">
        /// Throws when <paramref name="model"/> is not a valid model.
        /// </exception>
        public T PutModel<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            if (model is IInternalRealtimeModel internalModel)
            {
                internalModel.AttachRealtime(this, true);
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
        /// <returns>
        /// A <see cref="Task"/> that represents the provided <paramref name="model"/>.
        /// </returns>
        /// <exception cref="DatabaseInvalidModel">
        /// Throws when <paramref name="model"/> is not a valid model.
        /// </exception>
        public async Task<T> PutModelAsync<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            if (model is IInternalRealtimeModel internalModel)
            {
                await internalModel.AttachRealtimeAsync(this, true);
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
        /// <returns>
        /// The provided <paramref name="model"/>.
        /// </returns>
        /// <exception cref="DatabaseInvalidModel">
        /// Throws when <paramref name="model"/> is not a valid model.
        /// </exception>
        public T SubModel<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            if (model is IInternalRealtimeModel internalModel)
            {
                internalModel.AttachRealtime(this, false);
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
        /// <returns>
        /// A <see cref="Task"/> that represents the provided <paramref name="model"/>.
        /// </returns>
        /// <exception cref="DatabaseInvalidModel">
        /// Throws when <paramref name="model"/> is not a valid model.
        /// </exception>
        public async Task<T> SubModelAsync<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            if (model is IInternalRealtimeModel internalModel)
            {
                await internalModel.AttachRealtimeAsync(this, false);
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
                (string sync, string local, LocalDataChangesType changesType) = DBGetData(writeTask.Path);
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
            absolutePath = UrlUtilities.Separate(absoluteUrl.Substring(8));
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
        public RealtimeInstance Child(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

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
        public bool HasChildren(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            return InternalGetDataType(path) == LocalDataType.Path;
        }

        #endregion

        #region CRUD Methods

        /// <summary>
        /// Gets all sub children of the instance.
        /// </summary>
        /// <param name="path">
        /// The path of the sub paths.
        /// </param>
        /// <returns>
        /// The all sub paths.
        /// </returns>
        public string[][] GetAllChildren(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            return InternalGetAllChildren(path);
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
        public (string sync, string local, string value, LocalDataChangesType changesType) GetData(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            return InternalGetData(path);
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
        public string GetValue(params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            return InternalGetData(path).value;
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
        public bool SetValue(string value, params string[] path)
        {
            if (IsDisposed)
            {
                return default;
            }

            return MakeChanges(value, path);
        }

        #endregion

        #region Sync Helpers

        private protected (string sync, string local, string value, LocalDataChangesType changesType) InternalGetData(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            (string sync, string local, LocalDataChangesType changesType) = DBGetData(absoluteDataPath);
            string value = local == null ? sync : local;

            return (sync, local, value, changesType);
        }

        private protected LocalDataType InternalGetDataType(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return DBGetDataType(absoluteDataPath);
        }

        private protected string[][] InternalGetAllChildren(params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return DBGetRecursiveRelativeChildren(absoluteDataPath);
        }

        private protected bool MakeChanges(string value, params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return rwLock.LockWrite(absoluteDataPath, () =>
            {
                DBCancelPut(absoluteDataPath);

                (string sync, string local, LocalDataChangesType changesType) = DBGetData(absoluteDataPath);
                string oldValue = local == null ? sync : local;

                if (sync == null)
                {
                    if (value == null)
                    {
                        DBDeleteData(absoluteDataPath);
                    }
                    else
                    {
                        DBSetData(sync, value, LocalDataChangesType.Create, absoluteDataPath);
                    }
                    DBPut(value, absoluteDataPath);
                }
                else if (oldValue != value)
                {
                    if (value == null)
                    {
                        DBSetData(sync, null, LocalDataChangesType.Delete, absoluteDataPath);
                    }
                    else
                    {
                        DBSetData(sync, value, LocalDataChangesType.Update, absoluteDataPath);
                    }
                    DBPut(value, absoluteDataPath);
                }

                return oldValue != value;
            });
        }

        private protected bool MakeSync(string value, params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return rwLock.LockWrite(absoluteDataPath, () =>
            {
                DBCancelPut(absoluteDataPath);

                (string sync, string local, LocalDataChangesType changesType) = DBGetData(absoluteDataPath);
                string oldValue = local == null ? sync : local;

                if (local == null)
                {
                    if (value == null)
                    {
                        DBDeleteData(absoluteDataPath);
                    }
                    else
                    {
                        DBSetData(value, local, LocalDataChangesType.Create, absoluteDataPath);
                    }
                }
                else if (local == value)
                {
                    DBSetData(value, null, LocalDataChangesType.Synced, absoluteDataPath);
                }
                else
                {
                    switch (changesType)
                    {
                        case LocalDataChangesType.Create:
                            if (value == null)
                            {
                                DBPut(local, absoluteDataPath);
                            }
                            else
                            {
                                DBSetData(value, null, LocalDataChangesType.Synced, absoluteDataPath);
                            }
                            break;
                        case LocalDataChangesType.Update:
                            if (value == null)
                            {
                                DBDeleteData(absoluteDataPath);
                            }
                            else if (sync == value)
                            {
                                DBPut(local, absoluteDataPath);
                            }
                            else
                            {
                                DBSetData(value, null, LocalDataChangesType.Synced, absoluteDataPath);
                            }
                            break;
                        case LocalDataChangesType.Delete:
                            if (value == null)
                            {
                                break;
                            }
                            if (sync == value)
                            {
                                DBPut(null, absoluteDataPath);
                            }
                            else
                            {
                                DBSetData(value, null, LocalDataChangesType.Synced, absoluteDataPath);
                            }
                            break;
                        case LocalDataChangesType.Synced:
                            DBSetData(value, null, LocalDataChangesType.Synced, absoluteDataPath);
                            break;
                    }
                }

                return oldValue != value;
            });
        }

        private protected bool MakeSync(IDictionary<string[], string> values, params string[] path)
        {
            string[] absoluteDataPath = DBGetAbsoluteDataPath(path);

            return rwLock.LockWrite(absoluteDataPath, () =>
            {
                string[][] children = InternalGetAllChildren(path);
                foreach (string[] child in children)
                {
                    if (!values.ContainsKey(child))
                    {
                        MakeSync(default(string), child);
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

        #endregion

        #region DB Helpers

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

        private void DBDeleteData(params string[] path)
        {
            rwLock.LockWrite(path, () => App.LocalDatabase.InternalDelete(LocalDatabase, path));
        }

        private string[][] DBGetRecursiveRelativeChildren(params string[] path)
        {
            return rwLock.LockRead(path, () => App.LocalDatabase.InternalGetRecursiveRelativeChildren(LocalDatabase, path));
        }

        private LocalDataType DBGetDataType(params string[] path)
        {
            return rwLock.LockRead(path, () => App.LocalDatabase.InternalGetDataType(LocalDatabase, path));
        }

        private (string sync, string local, LocalDataChangesType changesType) DBGetData(params string[] path)
        {
            string data = rwLock.LockRead(path, () => App.LocalDatabase.InternalGetValue(LocalDatabase, path));

            if (string.IsNullOrEmpty(data))
            {
                return default;
            }

            string[] deserialized = StringUtilities.Deserialize(data);
            if (deserialized?.Length != 3)
            {
                return (default, default, default);
            }

            string sync = deserialized[0];
            string local = deserialized[1];
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

            return (sync, local, changesType);
        }

        private void DBSetData(string sync, string local, LocalDataChangesType changesType, params string[] path)
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

        private string[] DBGetAbsoluteDataPath(string[] path)
        {
            int pathLength = path?.Length ?? 0;
            string[] absoluteDataPath = new string[absolutePath.Length + pathLength + 1];
            absoluteDataPath[0] = FirebaseDatabaseApp.OfflineDatabaseLocalIndicator;
            Array.Copy(absolutePath, 0, absoluteDataPath, 1, absolutePath.Length);
            if (pathLength > 0)
            {
                Array.Copy(path, 0, absoluteDataPath, absolutePath.Length + 1, pathLength);
            }

            return absoluteDataPath;
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
            if (IsDisposed)
            {
                return true;
            }

            return GetValue() == null;
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
                Uri = "https://" + UrlUtilities.Combine(path);
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
                                    else
                                    {
                                        error?.Invoke(this, err);
                                    }
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
