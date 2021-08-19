﻿using ObservableHelpers;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Realtime
{
    /// <summary>
    /// Provides fluid implementations for firebase realtime database.
    /// </summary>
    public class RealtimeInstance : SyncContext, INullableObject
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
        /// The parent realtime instance of the instance.
        /// </summary>
        public RealtimeInstance Parent { get; }

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

        /// <summary>
        /// Gets the status of the syncing. Returnes <c>true</c> whether the node is fully synced; otherwise <c>false</c>.
        /// </summary>
        public bool IsSynced { get => GetTotalDataCount() == GetSyncedDataCount(); }

        #endregion

        #region Initializers

        private protected RealtimeInstance(RestfulFirebaseApp app, IFirebaseQuery query)
        {
            App = app;
            Query = query;
        }

        private protected RealtimeInstance(RestfulFirebaseApp app, RealtimeInstance parent, IFirebaseQuery query)
           : this(app, query)
        {
            Parent = parent;

            Parent.Disposing += Parent_Disposing;
            SubscribeToParent();
        }

        private protected RealtimeInstance(RestfulFirebaseApp app, RealtimeInstance parent, string path)
           : this(app, parent, parent.Query.Child(path))
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeToParent();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates a close of the instance.
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

            var clone = new RealtimeInstance(App, Parent, Query);
            clone.SyncOperation.SetContext(this);

            return clone;
        }

        /// <summary>
        /// Checks whether the instance has a child with a provided <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the child to check.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the instance has a child with a provided <paramref name="path"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool HasChild(string path)
        {
            if (IsDisposed)
            {
                return false;
            }

            var uri = Utils.UrlCombine(Query.GetAbsolutePath().Trim('/'), path);
            return App.Database.OfflineDatabase.GetDatas(uri, true).Any(i => i.Blob != null);
        }

        /// <summary>
        /// Creates new child instance with the provided <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The path of the child instance to create.
        /// </param>
        /// <returns>
        /// The created child instance.
        /// </returns>
        public RealtimeInstance Child(string path)
        {
            if (IsDisposed)
            {
                return default;
            }

            var childWire = new RealtimeInstance(App, this, path);
            childWire.SyncOperation.SetContext(this);

            return childWire;
        }

        /// <summary>
        /// Gets the total data cached of the instance.
        /// </summary>
        /// <returns>
        /// The total data count.
        /// </returns>
        public int GetTotalDataCount()
        {
            if (IsDisposed)
            {
                return 0;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Count();
        }

        /// <summary>
        /// Gets the total synced data cached of node instance.
        /// </summary>
        /// <returns>
        /// The total synced data count.
        /// </returns>
        public int GetSyncedDataCount()
        {
            if (IsDisposed)
            {
                return 0;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).Where(i => i.Changes == null).Count();
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
        /// <param name="timeout">
        /// The <see cref="TimeSpan"/> timeout of the created task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public async Task<bool> WaitForSynced(bool cancelOnError = false, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            if (IsDisposed)
            {
                return false;
            }

            bool cancel = false;
            void RealtimeInstance_Error(object sender, WireExceptionEventArgs e)
            {
                cancel = true;
            }
            if (cancelOnError)
            {
                Error += RealtimeInstance_Error;
            }
            async Task<bool> waitTask()
            {
                while (!IsSynced && !cancel && !(cancellationToken?.IsCancellationRequested ?? false))
                {
                    try
                    {
                        if (cancellationToken.HasValue)
                        {
                            await Task.Delay(500, cancellationToken.Value).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Delay(500).ConfigureAwait(false);
                        }
                    }
                    catch { }
                }
                return IsSynced;
            }
            bool result = timeout.HasValue
                ? await Task.Run(waitTask).WithTimeout(timeout.Value, false).ConfigureAwait(false)
                : await Task.Run(waitTask).ConfigureAwait(false);
            if (cancelOnError)
            {
                Error -= RealtimeInstance_Error;
            }
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Task"/> that will complete when the instance is fully synced.
        /// </summary>
        /// <param name="cancelOnError">
        /// Specify <c>true</c> whether the task will be cancelled on error; otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the fully sync status.
        /// </returns>
        public Task<bool> WaitForSynced(bool cancelOnError)
        {
            return WaitForSynced(cancelOnError: cancelOnError);
        }

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
            return WaitForSynced(timeout: timeout);
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
            return WaitForSynced(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sets blob to the specified node.
        /// </summary>
        /// <param name="blob">
        /// The blob to set.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the blob was set; otherwise, <c>false</c>.
        /// </returns>
        public bool SetBlob(string blob)
        {
            if (IsDisposed)
            {
                return false;
            }

            var hasChanges = false;

            var affectedUris = new List<string>();

            var uri = Query.GetAbsolutePath();

            // Delete related changes
            var subDatas = App.Database.OfflineDatabase.GetDatas(uri, false, true);
            foreach (var subData in subDatas)
            {
                if (subData.MakeChanges(null, err => OnPutError(subData, err)))
                {
                    hasChanges = true;
                    affectedUris.Add(subData.Uri);
                }
            }

            // Make changes
            var dataHolder = new DataHolder(App, uri);
            if (dataHolder.MakeChanges(blob, err => OnPutError(dataHolder, err)))
            {
                hasChanges = true;
                affectedUris.Add(uri);
            }

            if (hasChanges)
            {
                OnDataChanges(affectedUris.ToArray());
            }

            return hasChanges;
        }

        /// <inheritdoc/>
        public bool SetNull()
        {
            return SetBlob(null);
        }

        /// <inheritdoc/>
        public bool IsNull()
        {
            if (IsDisposed)
            {
                return true;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetDatas(uri, true).All(i => i.Blob == null);
        }

        /// <inheritdoc/>
        public string GetBlob()
        {
            if (IsDisposed)
            {
                return default;
            }

            var uri = Query.GetAbsolutePath();
            var dataHolder = new DataHolder(App, uri);
            return dataHolder.Blob;
        }

        /// <summary>
        /// Gets the cached sub paths of the instance.
        /// </summary>
        /// <returns>
        /// The cached sub paths.
        /// </returns>
        public IEnumerable<string> GetSubPaths()
        {
            if (IsDisposed)
            {
                return null;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false).Select(i => i.Replace(uri, "").Trim('/')).Where(i => !string.IsNullOrEmpty(i));
        }

        /// <summary>
        /// Gets the cached sub uris of the instance.
        /// </summary>
        /// <returns>
        /// The cached sub path of the instance.
        /// </returns>
        public IEnumerable<string> GetSubUris()
        {
            if (IsDisposed)
            {
                return null;
            }

            var uri = Query.GetAbsolutePath();
            return App.Database.OfflineDatabase.GetSubUris(uri, false);
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
        public T PutModel<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            model.AttachRealtime(this, true);
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
        public T SubModel<T>(T model)
            where T : IRealtimeModel
        {
            if (IsDisposed)
            {
                return model;
            }

            model.AttachRealtime(this, false);
            return model;
        }

        /// <summary>
        /// Gets the <see cref="string"/> representation of the instance.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the instance.
        /// </returns>
        public override string ToString()
        {
            return Query.GetAbsolutePath();
        }

        /// <summary>
        /// Invokes <see cref="DataChanges"/> event to the instance and to the parent instance.
        /// </summary>
        /// <param name="uris">
        /// The uris of the data that has changes.
        /// </param>
        protected void OnDataChanges(params string[] uris)
        {
            if (IsDisposed)
            {
                return;
            }

            if (Parent == null)
            {
                var affectedPaths = new List<string>();
                var baseUri = Query.GetAbsolutePath();
                affectedPaths.Add("");
                foreach (var u in uris)
                {
                    var uri = u.EndsWith("/") ? u : u + "/";
                    if (!uri.StartsWith(baseUri)) continue;
                    var path = uri.Replace(baseUri, "");
                    var separatedPath = Utils.UrlSeparate(path);
                    var eventPath = "";
                    for (int i = 0; i < separatedPath.Length; i++)
                    {
                        if (string.IsNullOrEmpty(eventPath)) eventPath = Utils.UrlCombine(separatedPath[i]);
                        else eventPath = Utils.UrlCombine(eventPath, separatedPath[i]);
                        if (!affectedPaths.Any(affectedPath => Utils.UrlCompare(affectedPath, eventPath))) affectedPaths.Add(eventPath);
                    }
                }
                foreach (var affectedPath in affectedPaths.OrderByDescending(i => i.Length))
                {
                    SelfDataChanges(new DataChangesEventArgs(baseUri, affectedPath));
                }
            }
            else
            {
                Parent.OnDataChanges(uris);
            }
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

            if (Parent == null)
            {
                SelfError(new WireExceptionEventArgs(uri, exception));
            }
            else
            {
                Parent.OnError(uri, exception);
            }
        }

        /// <summary>
        /// Subscribes instance events to parent instance if theres any.
        /// </summary>
        protected void SubscribeToParent()
        {
            if (IsDisposed)
            {
                return;
            }

            if (Parent != null)
            {
                Parent.DataChanges += Parent_DataChanges;
                Parent.Error += Parent_Error;
            }
        }

        /// <summary>
        /// Unsubscribes instance events to parent instance if theres any.
        /// </summary>
        protected void UnsubscribeToParent()
        {
            if (IsDisposed)
            {
                return;
            }

            if (Parent != null)
            {
                Parent.DataChanges -= Parent_DataChanges;
                Parent.Error -= Parent_Error;
            }
        }

        internal void OnPutError(DataHolder holder, RetryExceptionEventArgs err)
        {
            if (IsDisposed)
            {
                return;
            }

            var hasChanges = false;
            if (err.Exception is DatabaseUnauthorizedException ex)
            {
                if (holder.Sync == null)
                {
                    if (holder.Delete()) hasChanges = true;
                }
                else
                {
                    if (holder.DeleteChanges()) hasChanges = true;
                }
            }

            OnError(holder.Uri, err.Exception);
            if (hasChanges)
            {
                OnDataChanges(holder.Uri);
            }
        }

        private void Parent_DataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            string baseUri = Query.GetAbsolutePath().Trim('/');
            if (e.Uri.StartsWith(baseUri))
            {
                var path = e.Uri.Replace(baseUri, "");
                SelfDataChanges(new DataChangesEventArgs(baseUri, path));
            }
        }

        private void Parent_Error(object sender, WireExceptionEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            string baseUri = Query.GetAbsolutePath().Trim('/');
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

        private void SelfDataChanges(DataChangesEventArgs e)
        {
            ContextPost(delegate
            {
                DataChanges?.Invoke(this, e);
            });
        }

        private void SelfError(WireExceptionEventArgs e)
        {
            ContextPost(delegate
            {
                Error?.Invoke(this, e);
            });
        }

        #endregion
    }
}
