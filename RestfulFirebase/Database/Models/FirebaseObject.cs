﻿using ObservableHelpers;
using ObservableHelpers.Exceptions;
using ObservableHelpers.Utilities;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model <see cref="ObservableObject"/> for the <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/>.
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
        /// <param name="validate">
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
        /// <exception cref="DatabaseNullCascadeRealtimeModelException">
        /// Cascade IRealtimeModel cannot be null. Use IRealtimeModel.SetNull() instead.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        protected bool SetFirebasePropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (!(value is IRealtimeModel))
                {
                    throw new DatabaseNullCascadeRealtimeModelException();
                }
            }

            return base.SetPropertyWithKey(value, key, propertyName, nameof(FirebaseObject), validate, postAction);
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
        /// <param name="defaultValue">
        /// The default value of the property to set and return if the property is empty.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to get.
        /// </param>
        /// <param name="validate">
        /// The value set validator function.
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
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (!(defaultValue is IRealtimeModel))
                {
                    if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
                    {
                        throw new DatabaseInvalidCascadeRealtimeModelException();
                    }
                    defaultValue = (T)Activator.CreateInstance(typeof(T));
                }
            }

            return base.GetPropertyWithKey(key, defaultValue, propertyName, nameof(FirebaseObject), validate, postAction);
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
        /// <param name="validate">
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
        /// <exception cref="DatabaseNullCascadeRealtimeModelException">
        /// Cascade IRealtimeModel cannot be null. Use IRealtimeModel.SetNull() instead.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        protected bool SetFirebaseProperty<T>(
            T value,
            [CallerMemberName] string propertyName = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            return SetFirebasePropertyWithKey(value, propertyName, propertyName, validate, postAction);
        }

        /// <summary>
        /// Gets the firebase property value using <paramref name="propertyName"/> or the caller`s member name as its firebase key.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to get.
        /// </typeparam>
        /// <param name="defaultValue">
        /// The default value of the property to set and return if the property is empty.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to get.
        /// </param>
        /// <param name="validate">
        /// The value set validator function.
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
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T oldValue, T newValue), bool> validate = null,
            Action<(string key, string propertyName, string group, T oldValue, T newValue, bool HasChanges)> postAction = null)
        {
            return GetFirebasePropertyWithKey(propertyName, defaultValue, propertyName, validate, postAction);
        }

        #endregion

        #region ObservableObject Members

        /// <inheritdoc/>
        protected override NamedProperty NamedPropertyFactory(string key, string propertyName, string group)
        {
            if (IsDisposed)
            {
                return null;
            }

            return new NamedProperty()
            {
                Property = group == nameof(FirebaseObject) ? new FirebaseProperty() : new ObservableProperty(),
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
        }

        #endregion

        #region Disposable Members

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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
                        List<string> children = RealtimeInstance
                            .GetChildren()
                            .Select(i => i.key)
                            .ToList();

                        IEnumerable<NamedProperty> properties = GetRawProperties(nameof(FirebaseObject));

                        foreach (var property in properties)
                        {
                            if (property.Property is IInternalRealtimeModel model)
                            {
                                if (invokeSetFirst)
                                {
                                    RealtimeInstance.Child(property.Key).PutModel(model);
                                }
                                else
                                {
                                    RealtimeInstance.Child(property.Key).SubModel(model);
                                }
                            }
                            children.Remove(property.Key);
                        }

                        foreach (var child in children)
                        {
                            GetOrCreateNamedProperty(default(string), child, null, nameof(FirebaseObject),
                                newNamedProperty => true,
                                postAction =>
                                {
                                    if (postAction.namedProperty.Property is IInternalRealtimeModel model)
                                    {
                                        if (!model.HasAttachedRealtime)
                                        {
                                            RealtimeInstance.Child(child).SubModel(model);
                                        }
                                    }
                                });
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
                foreach (var item in GetRawProperties())
                {
                    if (item.Property is IInternalRealtimeModel model)
                    {
                        model.DetachRealtime();
                    }
                }

                var args = new RealtimeInstanceEventArgs(RealtimeInstance);

                Unsubscribe();

                OnRealtimeDetached(args);
            });
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

            if (e.Path.Length == 0)
            {
                return;
            }

            RWLock.LockWrite(() =>
            {
                GetOrCreateNamedProperty(default(string), e.Path[0], null, nameof(FirebaseObject),
                    newNamedProperty => RealtimeInstance.InternalGetData(e.Path[0]).HasValue,
                    postAction =>
                    {
                        if (postAction.namedProperty.Property is IInternalRealtimeModel model)
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
}
