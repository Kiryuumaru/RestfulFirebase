using ObservableHelpers;
using ObservableHelpers.Exceptions;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable object.
    /// </summary>
    public class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

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

        /// <inheritdoc/>
        public async void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                await attachLock.WaitAsync().ConfigureAwait(false);

                Subscribe(realtimeInstance);

                InitializeProperties();

                IEnumerable<NamedProperty> props = GetRawProperties(nameof(FirebaseObject));
                List<string> supPaths = new List<string>();
                foreach (var path in RealtimeInstance.GetSubPaths())
                {
                    var separatedPath = Utils.UrlSeparate(path);
                    var key = separatedPath[0];
                    if (!supPaths.Contains(key)) supPaths.Add(key);
                }

                foreach (var prop in props)
                {
                    if (invokeSetFirst)
                    {
                        RealtimeInstance.Child(prop.Key).PutModel((FirebaseProperty)prop.Property);
                    }
                    else
                    {
                        RealtimeInstance.Child(prop.Key).SubModel((FirebaseProperty)prop.Property);
                    }
                    supPaths.RemoveAll(i => i == prop.Key);
                }

                foreach (var path in supPaths)
                {
                    if (ExistsCore(path, null)) continue;
                    var namedProperty = NamedPropertyFactory(path, null, nameof(FirebaseObject));
                    if (namedProperty == null)
                    {
                        continue;
                    }
                    WireNamedProperty(namedProperty);
                    RealtimeInstance.Child(path).SubModel((FirebaseProperty)namedProperty.Property);
                    AddCore(namedProperty);
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

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        /// <inheritdoc/>
        public void DetachRealtime()
        {
            if (IsDisposed || !HasAttachedRealtime)
            {
                return;
            }

            foreach (var item in GetRawProperties())
            {
                if (item.Property is IRealtimeModel model)
                {
                    model.DetachRealtime();
                }
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        /// <inheritdoc/>
        public void LoadFromSerializedValue(string serialized)
        {
            if (IsDisposed)
            {
                return;
            }

            string[] properties = Utils.DeserializeString(serialized);

            if (properties == null)
            {
                return;
            }

            foreach (string data in properties)
            {
                string[] property = Utils.DeserializeString(data);
                if (property.Length != 4)
                {
                    continue;
                }
                string key = property[0];
                string name = property[1];
                string group = property[2];
                string value = property[3];
                try
                {
                    bool hasChanges = false;
                    NamedProperty namedProperty = GetCore(key, name);

                    if (namedProperty == null)
                    {
                        namedProperty = NamedPropertyFactory(key, name, group);
                        if (namedProperty != null)
                        {
                            WireNamedProperty(namedProperty);
                            AddCore(namedProperty);
                            ((FirebaseProperty)namedProperty.Property).LoadFromSerializedValue(value);
                            hasChanges = true;
                        }
                    }
                    else
                    {
                        namedProperty.Property.SyncOperation.SetContext(this);

                        if (namedProperty.Key != key)
                        {
                            namedProperty.Key = key;
                            hasChanges = true;
                        }

                        if (namedProperty.PropertyName != name)
                        {
                            namedProperty.PropertyName = name;
                            hasChanges = true;
                        }

                        if (namedProperty.Group != group)
                        {
                            namedProperty.Group = group;
                            hasChanges = true;
                        }

                        var hasSetChanges = false;

                        if (namedProperty.Property.SetValue(value))
                        {
                            hasSetChanges = true;
                            hasChanges = true;
                        }

                        if (!hasSetChanges && hasChanges)
                        {
                            OnPropertyChanged(namedProperty.Key, namedProperty.PropertyName, namedProperty.Group);
                        }
                    }

                }
                catch { }
            }
        }

        /// <inheritdoc/>
        public void LoadFromSerializedValue(string serialized, int[] encryptionPattern)
        {
            if (IsDisposed)
            {
                return;
            }

            var decrypted = Utils.DecryptString(serialized, encryptionPattern);
            LoadFromSerializedValue(decrypted);
        }

        /// <inheritdoc/>
        public string GenerateSerializedValue()
        {
            if (IsDisposed)
            {
                return default;
            }

            List<string> properties = new List<string>();
            foreach (NamedProperty namedProperty in GetRawProperties())
            {
                string data = Utils.SerializeString(
                    namedProperty.Key,
                    namedProperty.PropertyName,
                    namedProperty.Group,
                    ((FirebaseProperty)namedProperty.Property).GenerateSerializedValue());
                properties.Add(data);
            }

            return Utils.SerializeString(properties.ToArray());
        }

        /// <inheritdoc/>
        public string GenerateSerializedValue(int[] encryptionPattern)
        {
            if (IsDisposed)
            {
                return default;
            }

            string serialized = GenerateSerializedValue();
            if (serialized != null)
            {
                return Utils.EncryptString(serialized, encryptionPattern);
            }
            else
            {
                return null;
            }
        }

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
        /// <param name="onSet">
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
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
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

            return base.SetPropertyWithKey(value, key, propertyName, nameof(FirebaseObject), validate, onSet);
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
        /// <param name="onSet">
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
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
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

            return base.GetPropertyWithKey(key, defaultValue, propertyName, nameof(FirebaseObject), validate, onSet);
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
        /// <param name="onSet">
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
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            return SetFirebasePropertyWithKey(value, propertyName, propertyName, validate, onSet);
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
        /// <param name="onSet">
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
            EventHandler<ObjectPropertySetEventArgs<T>> onSet = null)
        {
            return GetFirebasePropertyWithKey(propertyName, defaultValue, propertyName, validate, onSet);
        }

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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
                foreach (var item in GetRawProperties())
                {
                    item.Property.Dispose();
                }
            }
            base.Dispose(disposing);
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

            if (!string.IsNullOrEmpty(e.Path))
            {
                var separated = Utils.UrlSeparate(e.Path);
                var key = separated[0];

                try
                {
                    await attachLock.WaitAsync().ConfigureAwait(false);
                    NamedProperty namedProperty = GetCore(key, null);
                    if (namedProperty == null && RealtimeInstance.HasChild(key))
                    {
                        namedProperty = NamedPropertyFactory(key, null, nameof(FirebaseObject));
                        if (namedProperty == null)
                        {
                            return;
                        }
                        WireNamedProperty(namedProperty);
                        RealtimeInstance.Child(key).SubModel((FirebaseProperty)namedProperty.Property);
                        AddCore(namedProperty);
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
    }
}
