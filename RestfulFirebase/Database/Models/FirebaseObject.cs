using ObservableHelpers;
using ObservableHelpers.Exceptions;
using ObservableHelpers.Utilities;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models;

/// <summary>
/// Provides an observable model <see cref="ObservableObject"/> for the <see cref="Realtime.RealtimeInstance"/>.
/// </summary>
public class FirebaseObject : ObservableObject, IInternalRealtimeModel
{
    #region Properties

    private bool? isInvokeToSetFirst;
    private bool hasPostAttachedRealtime;

    #endregion

    #region Initializers

    /// <summary>
    /// Creates new instance of <see cref="FirebaseObject"/> class.
    /// </summary>
    public FirebaseObject()
    {

    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets a firebase property value with the provided firebase <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the <paramref name="value"/> of the property to set.
    /// </typeparam>
    /// <param name="value">
    /// The value of the property to set.
    /// </param>
    /// <param name="key">
    /// The key of the property to set.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property to set.
    /// </param>
    /// <param name="setValidate">
    /// The value set validator function.
    /// </param>
    /// <param name="postAction">
    /// The callback after set operation.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the value of the property sets; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="PropertyKeyAndNameNullException">
    /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
    /// </exception>
    /// <exception cref="SerializerNotSupportedException">
    /// Occurs when the object has no supported serializer.
    /// </exception>
    protected bool SetFirebasePropertyWithKey<T>(
        T value,
        string? key,
        [CallerMemberName] string? propertyName = null,
        Func<bool>? setValidate = null,
        Action<(string? key, string? propertyName, string? group, T? oldValue, T? newValue, bool hasChanges)>? postAction = null)
    {
        if (IsDisposed)
        {
            return false;
        }

        if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
        {
            if (value == null)
            {
                if (setValidate?.Invoke() ?? true)
                {
                    T? prop = GetProperty(() =>
                    {
                        if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
                        {
                            return default;
                        }
                        return Activator.CreateInstance<T>();
                    }, key, propertyName, nameof(FirebaseObject), args =>
                    {
                        postAction?.Invoke((
                            args.key,
                            args.propertyName,
                            args.group,
                            args.oldValue,
                            args.newValue,
                            args.hasChanges));
                    });
                    if (prop is IRealtimeModel model)
                    {
                        return model.SetNull();
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        return SetProperty(value, key, propertyName, nameof(FirebaseObject), setValidate, postAction);
    }

    /// <summary>
    /// Gets the firebase property value of the provided firebase <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the property to get.
    /// </typeparam>
    /// <param name="key">
    /// The key of the property to get.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property to get.
    /// </param>
    /// <param name="postAction">
    /// The callback after set operation.
    /// </param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="PropertyKeyAndNameNullException">
    /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
    /// </exception>
    /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
    /// Cascade IRealtimeModel with no parameterless constructor should have a default value.
    /// </exception>
    /// <exception cref="SerializerNotSupportedException">
    /// Occurs when the object has no supported serializer.
    /// </exception>
    protected T? GetFirebasePropertyWithKey<T>(
        string? key,
        [CallerMemberName] string? propertyName = null,
        Action<(string? key, string? propertyName, string? group, T? oldValue, T? newValue, bool hasChanges)>? postAction = null)
    {
        if (IsDisposed)
        {
            return default;
        }

        if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
        {
            if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
            {
                throw new DatabaseInvalidCascadeRealtimeModelException();
            }
            T? prop = GetProperty(() => (T)Activator.CreateInstance(typeof(T)), key, propertyName, nameof(FirebaseObject), args =>
            {
                postAction?.Invoke((
                    args.key,
                    args.propertyName,
                    args.group,
                    args.oldValue,
                    args.newValue,
                    args.hasChanges));
            });
            if (prop is IRealtimeModel model && model.IsNull())
            {
                return default;
            }
            return prop;
        }
        else
        {
            return GetProperty(key, propertyName, nameof(FirebaseObject), postAction);
        }
    }

    /// <summary>
    /// Gets the firebase property value of the provided firebase <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the property to get.
    /// </typeparam>
    /// <param name="key">
    /// The key of the property to get.
    /// </param>
    /// <param name="defaultValueFactory">
    /// The default value factory that sets and returned if the property is null.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property to get.
    /// </param>
    /// <param name="postAction">
    /// The callback after set operation.
    /// </param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="PropertyKeyAndNameNullException">
    /// Throws when both <paramref name="key"/> and <paramref name="propertyName"/> are not provided.
    /// </exception>
    /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
    /// Cascade IRealtimeModel with no parameterless constructor should have a default value.
    /// </exception>
    /// <exception cref="SerializerNotSupportedException">
    /// Occurs when the object has no supported serializer.
    /// </exception>
    protected T GetFirebasePropertyWithKey<T>(
        string? key,
        Func<T> defaultValueFactory,
        [CallerMemberName] string? propertyName = null,
        Action<(string? key, string? propertyName, string? group, T? oldValue, T newValue, bool hasChanges)>? postAction = null)
    {
        if (IsDisposed)
        {
            return defaultValueFactory();
        }

        if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
        {
            if (defaultValueFactory == null)
            {
                if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new DatabaseInvalidCascadeRealtimeModelException();
                }
                defaultValueFactory = () => (T)Activator.CreateInstance(typeof(T));
            }
        }

        return GetProperty(defaultValueFactory, key, propertyName, nameof(FirebaseObject), postAction);
    }

    /// <summary>
    /// Sets a firebase property value using <paramref name="propertyName"/> or the caller`s member name as its firebase key.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the <paramref name="value"/> of the property to set.
    /// </typeparam>
    /// <param name="value">
    /// The value of the property to set.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property to set.
    /// </param>
    /// <param name="setValidate">
    /// The value set validator function.
    /// </param>
    /// <param name="postAction">
    /// The callback after set operation.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the value of the property sets; otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="PropertyKeyAndNameNullException">
    /// Throws when <paramref name="propertyName"/> is not provided.
    /// </exception>
    /// <exception cref="SerializerNotSupportedException">
    /// Occurs when the object has no supported serializer.
    /// </exception>
    protected bool SetFirebaseProperty<T>(
        T value,
        [CallerMemberName] string? propertyName = null,
        Func<bool>? setValidate = null,
        Action<(string? key, string? propertyName, string? group, T? oldValue, T? newValue, bool hasChanges)>? postAction = null)
    {
        return SetFirebasePropertyWithKey(value, propertyName, propertyName, setValidate, postAction);
    }

    /// <summary>
    /// Gets the firebase property value using <paramref name="propertyName"/> or the caller`s member name as its firebase key.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the property to get.
    /// </typeparam>
    /// <param name="propertyName">
    /// The name of the property to get.
    /// </param>
    /// <param name="postAction">
    /// The callback after set operation.
    /// </param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="PropertyKeyAndNameNullException">
    /// Throws when <paramref name="propertyName"/> is not provided.
    /// </exception>
    /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
    /// Cascade IRealtimeModel with no parameterless constructor should have a default value.
    /// </exception>
    /// <exception cref="SerializerNotSupportedException">
    /// Occurs when the object has no supported serializer.
    /// </exception>
    protected T? GetFirebaseProperty<T>(
        [CallerMemberName] string? propertyName = null,
        Action<(string? key, string? propertyName, string? group, T? oldValue, T? newValue, bool hasChanges)>? postAction = null)
    {
        return GetFirebasePropertyWithKey(propertyName, propertyName, postAction);
    }

    /// <summary>
    /// Gets the firebase property value using <paramref name="propertyName"/> or the caller`s member name as its firebase key.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying type of the property to get.
    /// </typeparam>
    /// <param name="defaultValueFactory">
    /// The default value factory that sets and returned if the property is null.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property to get.
    /// </param>
    /// <param name="postAction">
    /// The callback after set operation.
    /// </param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="PropertyKeyAndNameNullException">
    /// Throws when <paramref name="propertyName"/> is not provided.
    /// </exception>
    /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
    /// Cascade IRealtimeModel with no parameterless constructor should have a default value.
    /// </exception>
    /// <exception cref="SerializerNotSupportedException">
    /// Occurs when the object has no supported serializer.
    /// </exception>
    protected T GetFirebaseProperty<T>(
        Func<T> defaultValueFactory,
        [CallerMemberName] string? propertyName = null,
        Action<(string? key, string? propertyName, string? group, T? oldValue, T newValue, bool hasChanges)>? postAction = null)
    {
        return GetFirebasePropertyWithKey(propertyName, defaultValueFactory, propertyName, postAction);
    }

    #endregion

    #region ObservableObject Members

    /// <inheritdoc/>
    protected override NamedProperty NamedPropertyFactory(string? key, string? propertyName, string? group)
    {
        return new NamedProperty(
            group == nameof(FirebaseObject) ? new FirebaseProperty() : new ObservableProperty(),
            key, propertyName, group);
    }

    #endregion

    #region Disposable Members

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            RealtimeAttached = null;
            RealtimeDetached = null;
            WireError = null;

            Unsubscribe();

            RWLock.LockWrite(() =>
            {
                if (this is IInternalRealtimeModel model)
                {
                    model.DetachRealtime();
                }
            });
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

                List<string> children = realtimeInstance
                    .GetChildren()
                    .Select(i => i.key)
                    .ToList();

                IEnumerable<NamedProperty> properties = GetRawProperties(nameof(FirebaseObject));

                foreach (var property in properties)
                {
                    if (property.Key == null)
                    {
                        continue;
                    }
                    if (property.Property is IInternalRealtimeModel model)
                    {
                        if (invokeSetFirst)
                        {
                            realtimeInstance.Child(property.Key).PutModel(model);
                        }
                        else
                        {
                            realtimeInstance.Child(property.Key).SubModel(model);
                        }
                    }
                    children.Remove(property.Key);
                }

                foreach (var child in children)
                {
                    GetOrAddNamedProperty<string?>(child, null, nameof(FirebaseObject),
                        () => null,
                        args =>
                        {
                            if (args.namedProperty.Property is IInternalRealtimeModel model)
                            {
                                if (!model.HasAttachedRealtime)
                                {
                                    realtimeInstance.Child(child).SubModel(model);
                                }
                            }
                        });
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

            foreach (var item in GetRawProperties())
            {
                if (item.Property is IInternalRealtimeModel model)
                {
                    model.DetachRealtime();
                }
            }

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

        if (RealtimeInstance == null)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            if (IsDisposed)
            {
                return;
            }

            if (RealtimeInstance == null)
            {
                return;
            }

            RealtimeInstance.ImmediateDataChanges -= RealtimeInstance_ImmediateDataChanges;
            RealtimeInstance.Error -= RealtimeInstance_Error;
            RealtimeInstance.Disposing -= RealtimeInstance_Disposing;

            RealtimeInstance = null;
            isInvokeToSetFirst = null;
        });
    }

    private void RealtimeInstance_ImmediateDataChanges(object sender, DataChangesEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (e.Path.Length == 0)
        {
            return;
        }

        if (RealtimeInstance == null)
        {
            return;
        }

        RWLock.LockWrite(() =>
        {
            if (RealtimeInstance == null)
            {
                return;
            }

            CreateOrUpdateNamedProperty<string?>(e.Path[0], null, nameof(FirebaseObject),
                () => null,
                args => RealtimeInstance.InternalGetData(e.Path[0]).HasValue,
                args => false,
                args =>
                {
                    if (args.namedProperty.Property is IInternalRealtimeModel model)
                    {
                        if (!model.HasAttachedRealtime)
                        {
                            RealtimeInstance.Child(e.Path[0]).SubModel(model);
                        }
                    }
                });
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
