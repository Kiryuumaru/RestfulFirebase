using ObservableHelpers;
using ObservableHelpers.Exceptions;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Utilities;
using System;
using System.Collections.Generic;
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

        private SemaphoreSlim attachLock = new SemaphoreSlim(1, 1);

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
                Property = new FirebaseProperty(),
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
                foreach (var item in GetRawProperties())
                {
                    item.Property.Dispose();
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

            try
            {
                await attachLock.WaitAsync().ConfigureAwait(false);

                List<Task> tasks = new List<Task>();

                Subscribe(realtimeInstance);

                List<string> subPaths = new List<string>();
                //foreach (var path in RealtimeInstance.GetSubPaths())
                //{
                //    int keyLastIndex = path.IndexOf('/');
                //    string key = keyLastIndex == -1 ? path : path.Substring(0, keyLastIndex);
                //    subPaths.Add(key);
                //}

                foreach (var prop in GetRawProperties(nameof(FirebaseObject)))
                {
                    if (invokeSetFirst)
                    {
                        tasks.Add(RealtimeInstance.Child(prop.Key).PutModelAsync((FirebaseProperty)prop.Property));
                    }
                    else
                    {
                        tasks.Add(RealtimeInstance.Child(prop.Key).SubModelAsync((FirebaseProperty)prop.Property));
                    }
                    subPaths.RemoveAll(i => i == prop.Key);
                }

                foreach (var path in subPaths)
                {
                    GetOrCreateNamedProperty(default(string), path, null, nameof(FirebaseObject),
                        newNamedProperty => true,
                        postAction =>
                        {
                            if (postAction.namedProperty.Property is FirebaseProperty firebaseProperty)
                            {
                                if (!firebaseProperty.HasAttachedRealtime)
                                {
                                    RealtimeInstance.Child(path).SubModel(firebaseProperty);
                                }
                            }
                        });
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

            //if (string.IsNullOrEmpty(e.Path))
            //{
            //    return;
            //}

            //int keyLastIndex = e.Path.IndexOf('/');
            //string key = keyLastIndex == -1 ? e.Path : e.Path.Substring(0, keyLastIndex);

            //try
            //{
            //    await attachLock.WaitAsync().ConfigureAwait(false);
            //    GetOrCreateNamedProperty(default(string), key, null, nameof(FirebaseObject),
            //        newNamedProperty => RealtimeInstance.HasChild(key),
            //        postAction =>
            //        {
            //            if (postAction.namedProperty.Property is FirebaseProperty firebaseProperty)
            //            {
            //                if (!firebaseProperty.HasAttachedRealtime)
            //                {
            //                    RealtimeInstance.Child(key, false).SubModel(firebaseProperty);
            //                }
            //            }
            //        });
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
    }
}
