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
using ObservableHelpers.Utilities;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model <see cref="ObservableProperty"/> for the <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/>.
    /// </summary>
    public class FirebaseProperty : ObservableProperty, IInternalRealtimeModel
    {
        #region Properties

        private object currentValue;
        private Type currentType;
        private bool isValueCached;
        private bool? isInvokeToSetFirst;
        private bool hasPostAttachedRealtime;

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseProperty{T}"/> class.
        /// </summary>
        public FirebaseProperty()
        {

        }

        #endregion

        #region Methods



        #endregion

        #region ObservableProperty Members

        /// <summary>
        /// Internal implementation for <see cref="ObservableProperty.SetObject(Type, object)"/>.
        /// </summary>
        /// <param name="type">
        /// Underlying type of the object to set.
        /// </param>
        /// <param name="obj">
        /// The value object of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        protected override bool InternalSetObject(Type type, object obj)
        {
            if (IsDisposed)
            {
                return false;
            }

            bool hasObjChanges = false;

            if (obj is IInternalRealtimeModel model)
            {
                if (base.InternalSetObject(null, obj))
                {
                    if (currentType != null)
                    {
                        hasObjChanges = !(currentValue?.Equals(obj) ?? obj == null);
                    }
                    else
                    {
                        hasObjChanges = true;
                    }

                    currentValue = obj;
                    currentType = type;
                    isValueCached = true;

                    if (HasAttachedRealtime)
                    {
                        if (model.RealtimeInstance != RealtimeInstance)
                        {
                            model.AttachRealtime(RealtimeInstance, true);
                        }
                    }
                }
            }
            else
            {
                string blob = type == null ? null : Serializer.Serialize(obj, type);

                if (base.InternalSetObject(null, blob))
                {
                    if (currentType != null)
                    {
                        hasObjChanges = !(currentValue?.Equals(obj) ?? obj == null);
                    }
                    else
                    {
                        hasObjChanges = true;
                    }

                    currentValue = obj;
                    currentType = type;
                    isValueCached = true;

                    RealtimeInstanceSetBlob(blob);
                }
            }

            return hasObjChanges;
        }

        /// <summary>
        /// Internal implementation for <see cref="ObservableProperty.GetObject(Type, object)"/>.
        /// </summary>
        /// <param name="type">
        /// Underlying type of the object to get.
        /// </param>
        /// <returns>
        /// The value object of the property.
        /// </returns>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        protected override object InternalGetObject(Type type)
        {
            if (type == null)
            {
                return base.InternalGetObject(null);
            }
            else if (type == currentType && isValueCached)
            {
                return currentValue;
            }
            else
            {
                object obj = base.InternalGetObject(null);

                if (obj is IInternalRealtimeModel model)
                {
                    return model;
                }
                else
                {
                    string blob = obj as string;

                    currentValue = Serializer.Deserialize(blob, type, default);
                    currentType = type;
                    isValueCached = true;

                    return currentValue;
                }
            }
        }

        #endregion

        #region Disposable Members

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this is IInternalRealtimeModel model)
                {
                    model.DetachRealtime();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region IInternalRealtimeModel Members

        bool? IInternalRealtimeModel.IsInvokeToSetFirst => isInvokeToSetFirst;

        bool IInternalRealtimeModel.HasPostAttachedRealtime => hasPostAttachedRealtime;

        void IInternalRealtimeModel.AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            Subscribe(realtimeInstance, invokeSetFirst);

            bool isStaring = false;

            Task.Run(() =>
            {
                try
                {
                    RWLock.LockWrite(() =>
                    {
                        isStaring = true;

                        object obj = base.InternalGetObject(null);

                        if (obj is IInternalRealtimeModel model)
                        {
                            model.AttachRealtime(realtimeInstance, invokeSetFirst);
                        }
                        else
                        {
                            string blob = obj as string;

                            if (invokeSetFirst)
                            {
                                RealtimeInstanceSetBlob(blob);
                            }
                            else
                            {
                                blob = RealtimeInstance.GetValue();

                                object oldValue = Value;

                                if (base.InternalSetObject(null, blob))
                                {
                                    bool hasObjChanges = false;

                                    if (currentType != null)
                                    {
                                        object value = Serializer.Deserialize(blob, currentType, default);
                                        hasObjChanges = !(currentValue?.Equals(value) ?? value == null);
                                        currentValue = value;
                                        isValueCached = true;
                                    }
                                    else
                                    {
                                        hasObjChanges = true;
                                        isValueCached = false;
                                    }

                                    if (hasObjChanges)
                                    {
                                        OnPropertyChanged(nameof(Value));
                                    }
                                }
                            }
                        }

                        hasPostAttachedRealtime = true;

                        OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
                    });
                }
                catch
                {
                    Unsubscribe();
                    throw;
                }
            });

            while (!isStaring)
            {
                Thread.Sleep(1);
            }
        }

        void IInternalRealtimeModel.DetachRealtime()
        {
            if (IsDisposed || !HasAttachedRealtime)
            {
                return;
            }

            RWLock.LockWrite(() =>
            {
                if (base.InternalGetObject(null) is IInternalRealtimeModel model)
                {
                    model.DetachRealtime();
                }

                var args = new RealtimeInstanceEventArgs(RealtimeInstance);

                Unsubscribe();

                OnRealtimeDetached(args);
            });
        }

        private void RealtimeInstanceSetBlob(string blob)
        {
            RealtimeInstance instance = RealtimeInstance;
            if (instance != null)
            {
                if (!hasPostAttachedRealtime)
                {
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            instance = RealtimeInstance;
                            if (instance == null || hasPostAttachedRealtime)
                            {
                                break;
                            }
                            await Task.Delay(instance.App.Config.DatabaseRetryDelay);
                        }

                        instance?.SetValue(blob);
                    });
                }
                else
                {
                    instance.SetValue(blob);
                }
            }
        }

        private void Subscribe(RealtimeInstance realtimeInstance, bool invokeSetFirst)
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
            isInvokeToSetFirst = invokeSetFirst;
            hasPostAttachedRealtime = false;

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
            isInvokeToSetFirst = null;
            hasPostAttachedRealtime = false;
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            RWLock.LockWrite(() =>
            {
                if (!(base.InternalGetObject(null) is IInternalRealtimeModel))
                {
                    string blob = RealtimeInstance.GetValue();

                    object oldValue = Value;

                    if (base.InternalSetObject(null, blob))
                    {
                        bool hasObjChanges = false;

                        if (currentType != default)
                        {
                            object value = Serializer.Deserialize(blob, currentType, default);
                            hasObjChanges = !(currentValue?.Equals(value) ?? value == null);
                            currentValue = value;
                            isValueCached = true;
                        }
                        else
                        {
                            isValueCached = false;
                            hasObjChanges = true;
                        }

                        if (hasObjChanges)
                        {
                            OnPropertyChanged(nameof(Value));
                        }
                    }
                }
            });
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
    }
    /// <summary>
    /// Provides an observable model <see cref="ObservableProperty{T}"/> for the <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/>.
    /// </summary>
    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Properties

        /// <inheritdoc/>
        public new T Value
        {
            get => GetValue<T>();
            set => SetValue<T>(value);
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
            if (!typeof(IInternalRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (!Serializer.CanSerialize<T>())
                {
                    throw new SerializerNotSupportedException(typeof(T));
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
        /// </returns>
        public bool SetValue(T value) => SetValue<T>(value);

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value return if the property is disposed or null.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        public T GetValue(T defaultValue = default) => GetValue<T>(defaultValue);

        #endregion
    }
}
