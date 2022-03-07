using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RestfulFirebase.Local;
using ObservableHelpers.Utilities;
using SerializerHelpers;
using SerializerHelpers.Exceptions;

namespace RestfulFirebase.Database.Models;

/// <summary>
/// Provides an observable model <see cref="ObservableProperty"/> for the <see cref="Realtime.RealtimeInstance"/>.
/// </summary>
public class FirebaseProperty : ObservableProperty, IInternalRealtimeModel
{
    #region Properties

    private object? currentValue;
    private Type? currentType;
    private bool isValueCached;
    private bool? isInvokeToSetFirst;
    private bool hasPostAttachedRealtime;

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
    protected override bool InternalSetObject(Type? type, object? obj)
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

                if (RealtimeInstance != null)
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
            string? blob = type == null ? null : Serializer.Serialize(obj, type);

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

                RealtimeInstance? instance = RealtimeInstance;
                if (instance != null && !instance.IsDisposed)
                {
                    if (hasPostAttachedRealtime)
                    {
                        Task.Run(async () =>
                        {
                            while (true)
                            {
                                if (!hasPostAttachedRealtime)
                                {
                                    instance = RealtimeInstance;
                                    if (instance?.IsDisposed ?? true)
                                    {
                                        return;
                                    }
                                    break;
                                }
                                await Task.Delay(instance.App.Config.CachedDatabaseRetryDelay);
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
        }

        return hasObjChanges;
    }

    /// <summary>
    /// Internal implementation for <see cref="ObservableProperty.GetObject(Type?, Func{object?}?)"/>.
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
    protected override object? InternalGetObject(Type? type)
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
            object? obj = base.InternalGetObject(null);

            if (obj is IInternalRealtimeModel model)
            {
                return model;
            }
            else
            {
                string? blob = obj as string;

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
        if (IsDisposed && !realtimeInstance.IsDisposed)
        {
            return;
        }

        hasPostAttachedRealtime = true;

        RWLock.LockWriteAndForget(() =>
        {
            try
            {
                if (IsDisposed && !realtimeInstance.IsDisposed)
                {
                    return;
                }

                object? obj = base.InternalGetObject(null);

                if (obj is IInternalRealtimeModel model)
                {
                    model.AttachRealtime(realtimeInstance, invokeSetFirst);
                }
                else
                {
                    string? blob = obj as string;

                    if (invokeSetFirst)
                    {
                        realtimeInstance.SetValue(blob);
                    }
                    else
                    {
                        blob = realtimeInstance.GetValue();

                        object? oldValue = Value;

                        if (base.InternalSetObject(null, blob))
                        {
                            bool hasObjChanges = false;

                            if (currentType != null)
                            {
                                object? value = Serializer.Deserialize(blob, currentType, default);
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
                hasPostAttachedRealtime = false;
            }
        });
    }

    void IInternalRealtimeModel.DetachRealtime()
    {
        var realtimeInstance = RealtimeInstance;
        if (IsDisposed || realtimeInstance == null)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            if (base.InternalGetObject(null) is IInternalRealtimeModel model)
            {
                model.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(realtimeInstance);

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
            isInvokeToSetFirst = invokeSetFirst;

            RealtimeInstance.ImmediateDataChanges += RealtimeInstance_ImmediateDataChanges;
            RealtimeInstance.Error += RealtimeInstance_Error;
            RealtimeInstance.Disposing += RealtimeInstance_Disposing;
        });
    }

    private void Unsubscribe()
    {
        if (IsDisposed)
        {
            return;
        }

        if (!HasAttachedRealtime)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            if (IsDisposed)
            {
                return;
            }

            var realtimeInstance = RealtimeInstance;
            if (realtimeInstance == null)
            {
                return;
            }

            realtimeInstance.ImmediateDataChanges -= RealtimeInstance_ImmediateDataChanges;
            realtimeInstance.Error -= RealtimeInstance_Error;
            realtimeInstance.Disposing -= RealtimeInstance_Disposing;

            RealtimeInstance = null;
            isInvokeToSetFirst = null;
        });
    }

    private void RealtimeInstance_ImmediateDataChanges(object? sender, DataChangesEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            var realtimeInstance = RealtimeInstance;
            if (realtimeInstance == null)
            {
                return;
            }

            if (base.InternalGetObject(null) is not IInternalRealtimeModel)
            {
                string? blob = realtimeInstance.GetValue();

                object? oldValue = Value;

                if (base.InternalSetObject(null, blob))
                {
                    bool hasObjChanges = false;

                    if (currentType != default)
                    {
                        object? value = Serializer.Deserialize(blob, currentType, default);
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
                        RWLock.InvokeOnLockExit(() => OnPropertyChanged(nameof(Value)));
                    }
                }
            }
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

        (this as IInternalRealtimeModel).DetachRealtime();
    }

    #endregion

    #region IRealtimeModel Members

    /// <inheritdoc/>
    public RealtimeInstance? RealtimeInstance { get; private set; }

    /// <inheritdoc/>
    public bool HasAttachedRealtime { get => !(RealtimeInstance?.IsDisposed ?? true); }

    /// <inheritdoc/>
    public event EventHandler<RealtimeInstanceEventArgs>? RealtimeAttached;

    /// <inheritdoc/>
    public event EventHandler<RealtimeInstanceEventArgs>? RealtimeDetached;

    /// <inheritdoc/>
    public event EventHandler<WireExceptionEventArgs>? WireError;

    /// <summary>
    /// Invokes <see cref="RealtimeAttached"/> event on the current context.
    /// </summary>
    /// <param name="args">
    /// The event arguments for the event to invoke.
    /// </param>
    protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
    {
        ContextPost(delegate
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
        ContextPost(delegate
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
/// Provides an observable model <see cref="ObservableProperty{T}"/> for the <see cref="RealtimeInstance"/>.
/// </summary>
public class FirebaseProperty<T> : FirebaseProperty
{
    #region Properties

    /// <inheritdoc/>
    public new T? Value
    {
        get => GetValue();
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
    /// <returns>
    /// The value of the property.
    /// </returns>
    public T? GetValue() => GetValue<T>();

    /// <summary>
    /// Gets the value of the property.
    /// </summary>
    /// <param name="defaultValueFactory">
    /// The default value factory if the property is disposed or null.
    /// </param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    public T GetValue(Func<T> defaultValueFactory) => GetValue<T>(defaultValueFactory);

    #endregion
}
