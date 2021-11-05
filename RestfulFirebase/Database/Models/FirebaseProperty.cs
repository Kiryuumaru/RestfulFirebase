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
using RestfulFirebase.Local;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model <see cref="ObservableProperty"/> for the <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/>.
    /// </summary>
    public class FirebaseProperty : ObservableProperty, IInternalRealtimeModel
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

            if (typeof(IInternalRealtimeModel).IsAssignableFrom(type))
            {
                if (obj is IInternalRealtimeModel model)
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
                        RealtimeInstance.SetValue(blob);
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
                if (base.GetObject() is IInternalRealtimeModel model)
                {
                    model.Dispose();
                }
                (this as IInternalRealtimeModel).DetachRealtime();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region IInternalRealtimeModel Members

        async void IInternalRealtimeModel.AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            await (this as IInternalRealtimeModel).AttachRealtimeAsync(realtimeInstance, invokeSetFirst);
        }

        async Task IInternalRealtimeModel.AttachRealtimeAsync(RealtimeInstance realtimeInstance, bool invokeSetFirst)
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

                if (obj is IInternalRealtimeModel model)
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
                        RealtimeInstance.SetValue(blob);
                    }
                    else
                    {
                        base.SetObject(RealtimeInstance.GetValue(), typeof(string));
                    }
                }

                await Task.WhenAll(tasks);
            }
            finally
            {
                attachLock.Release();
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        void IInternalRealtimeModel.DetachRealtime()
        {
            if (IsDisposed || !HasAttachedRealtime)
            {
                return;
            }

            if (base.GetObject() is IInternalRealtimeModel model)
            {
                model.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
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

            //if (e.Path.IndexOf('/') != -1)
            //{
            //    return;
            //}

            //try
            //{
            //    await attachLock.WaitAsync().ConfigureAwait(false);
            //    if (!(base.GetObject() is IRealtimeModel))
            //    {
            //        base.SetObject(RealtimeInstance.GetValue(), typeof(string));
            //    }
            //}
            //finally
            //{
            //    attachLock.Release();
            //}
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

            (this as IInternalRealtimeModel).DetachRealtime();
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
                        RealtimeInstance.SetValue(null);
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
