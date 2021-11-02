using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using RestfulFirebase.Serializers;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable property.
    /// </summary>
    public class FirebaseProperty : ObservableProperty, IRealtimeModel
    {
        #region Properties

        private SemaphoreSlim attachLock = new SemaphoreSlim(1, 1);

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseProperty"/> class.
        /// </summary>
        public FirebaseProperty()
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
        public async Task<bool> WaitForSynced(bool cancelOnError = false, CancellationToken? cancellationToken = null)
        {
            return await RealtimeInstance.WaitForSynced(cancelOnError, cancellationToken);
        }

        /// <summary>
        /// Invokes <see cref="RealtimeAttached"/> event on the current context.
        /// </summary>
        /// <param name="args">
        /// The event arguments for the event to invoke.
        /// </param>
        protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            ContextSend(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        /// <summary>
        /// Invokes <see cref="RealtimeDetached"/> event on the current context.
        /// </summary>
        /// <param name="args">
        /// The event arguments for the event to invoke.
        /// </param>
        protected virtual void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            ContextSend(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
        }

        /// <summary>
        /// Invokes <see cref="WireError"/> event on the current context.
        /// </summary>
        /// <param name="args">
        /// The event arguments for the event to invoke.
        /// </param>
        protected virtual void OnWireError(WireExceptionEventArgs args)
        {
            ContextPost(delegate
            {
                WireError?.Invoke(this, args);
            });
        }

        private void Subscribe(RealtimeInstance realtimeInstance)
        {
            if (IsDisposed)
            {
                return;
            }

            if (HasAttachedRealtime)
            {
                Unsubscribe();
            }

            RealtimeInstance = realtimeInstance;

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
                RealtimeInstance.Disposing += RealtimeInstance_Disposing;
            }
        }

        private void Unsubscribe()
        {
            if (IsDisposed)
            {
                return;
            }

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
                RealtimeInstance.Disposing -= RealtimeInstance_Disposing;
            }

            RealtimeInstance = null;
        }

        private async void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            var path = UrlUtilities.Separate(e.Path);

            if (path.Length == 0)
            {
                try
                {
                    await attachLock.WaitAsync().ConfigureAwait(false);
                    if (!(base.GetObject() is IRealtimeModel))
                    {
                        base.SetObject(RealtimeInstance.GetBlob(), typeof(string));
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    attachLock.Release();
                }
            }
        }

        private void RealtimeInstance_Error(object sender, WireExceptionEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            OnWireError(e);
        }

        private void RealtimeInstance_Disposing(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            DetachRealtime();
        }

        #endregion

        #region ObservableProperty Members

        /// <summary>
        /// Sets the object of the property.
        /// </summary>
        /// <param name="obj">
        /// The value object of the property.
        /// </param>
        /// <param name="type">
        /// The underlying type of the object.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        protected override bool SetObject(object obj, Type type)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(type))
            {
                if (obj is IRealtimeModel model)
                {
                    if (HasAttachedRealtime)
                    {
                        model.AttachRealtime(RealtimeInstance, true);
                    }
                }

                return base.SetObject(obj, type);
            }
            else
            {
                string blob = Serializer.Serialize(obj, type);

                if (base.SetObject(blob, typeof(string)))
                {
                    if (HasAttachedRealtime)
                    {
                        RealtimeInstance.SetBlob(blob);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the value object of the property.
        /// </summary>
        /// <returns>
        /// The value object of the property.
        /// </returns>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        protected override object GetObject(Type type = null)
        {
            if (IsDisposed)
            {
                return default;
            }

            object obj = base.GetObject(type);

            if (type == null || typeof(IRealtimeModel).IsAssignableFrom(type))
            {
                if (obj is IRealtimeModel)
                {
                    return obj;
                }
            }

            if (obj is string objBlob)
            {
                return Serializer.Deserialize(objBlob, type, default);
            }
            else if (obj is null)
            {
                return default;
            }
            else
            {
                throw new SerializerNotSupportedException(obj.GetType());
            }
        }

        #endregion

        #region Disposable Members

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var obj = base.GetObject();

                DetachRealtime();

                if (obj is IDisposable model)
                {
                    model.Dispose();
                }

                base.SetObject(null);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region IRealtimeModel Members

        /// <inheritdoc/>
        public RealtimeInstance RealtimeInstance { get; private set; }

        /// <inheritdoc/>
        public bool HasAttachedRealtime { get => !(RealtimeInstance?.IsDisposed ?? true); }

        /// <inheritdoc/>
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;

        /// <inheritdoc/>
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;

        /// <inheritdoc/>
        public event EventHandler<WireExceptionEventArgs> WireError;

        /// <inheritdoc/>
        public async void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            await AttachRealtimeAsync(realtimeInstance, invokeSetFirst);
        }

        /// <inheritdoc/>
        public async Task AttachRealtimeAsync(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            var obj = base.GetObject();

            try
            {
                await attachLock.WaitAsync().ConfigureAwait(false);

                List<Task> tasks = new List<Task>();

                Subscribe(realtimeInstance);

                if (obj is IRealtimeModel model)
                {
                    tasks.Add(model.AttachRealtimeAsync(realtimeInstance, invokeSetFirst));
                }
                else
                {
                    string blob = null;
                    if (obj is string objBlob)
                    {
                        blob = objBlob;
                    }
                    else if (obj is null)
                    {
                        blob = null;
                    }
                    else
                    {
                        throw new SerializerNotSupportedException(obj.GetType());
                    }

                    if (invokeSetFirst)
                    {
                        RealtimeInstance.SetBlob(blob);
                    }
                    else
                    {
                        base.SetObject(RealtimeInstance.GetBlob(), typeof(string));
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch
            {
                throw;
            }
            finally
            {
                attachLock.Release();
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        /// <inheritdoc/>
        public void DetachRealtime()
        {
            if (IsDisposed || !HasAttachedRealtime)
            {
                return;
            }

            if (base.GetObject() is IRealtimeModel model)
            {
                model.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        #endregion

        #region INullableObject Members

        /// <inheritdoc/>
        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return false;
            }

            if (base.GetObject() is IRealtimeModel)
            {
                return base.SetNull();
            }
            else
            {
                if (base.SetNull())
                {
                    if (HasAttachedRealtime)
                    {
                        RealtimeInstance.SetBlob(null);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion
    }

    /// <inheritdoc/>
    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Properties

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public new T Value
        {
            get => base.GetValue<T>(default);
            set => base.SetValue(value);
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseProperty{T}"/> class.
        /// </summary>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        public FirebaseProperty()
        {
            if (!Serializer.CanSerialize<T>())
            {
                throw new SerializerNotSupportedException(typeof(T));
            }
        }

        #endregion
    }
}
