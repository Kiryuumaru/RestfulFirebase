using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
        public event EventHandler<WireException> WireError;

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
        public void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            lock (this)
            {
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
                    if (invokeSetFirst) RealtimeInstance.Child(prop.Key).PutModel((FirebaseProperty)prop.Property);
                    else RealtimeInstance.Child(prop.Key).SubModel((FirebaseProperty)prop.Property);
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

        /// <summary>
        /// Sets a property for the firebase object.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the <paramref name="value"/> of the property.
        /// </typeparam>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="key">
        /// The key of the property.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the value of the property sets; otherwise <c>false</c>.
        /// </returns>
        protected bool SetFirebasePropertyWithKey<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (!(value is IRealtimeModel))
                {
                    throw new Exception("Cascade IRealtimeModel cannot be null. Use IRealtimeModel.SetNull() instead.");
                }
            }

            return base.SetPropertyWithKey(value, key, propertyName, nameof(FirebaseObject));
        }

        /// <summary>
        /// Sets a property for the firebase object.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to get.
        /// </typeparam>
        /// <param name="key">
        /// The key of the property.
        /// </param>
        /// <param name="defaultValue">
        /// The default value of the property to set and return if the property is empty.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        protected T GetFirebasePropertyWithKey<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null)
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
                        throw new Exception("Cascade IRealtimeModel with no parameterless constructor should have a default value.");
                    }
                    defaultValue = (T)Activator.CreateInstance(typeof(T));
                }
            }

            return base.GetPropertyWithKey(key, defaultValue, propertyName, nameof(FirebaseObject));
        }

        /// <summary>
        /// Sets a property for the firebase object.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the <paramref name="value"/> of the property.
        /// </typeparam>
        /// <param name="value">
        /// The value of the property.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the value of the property sets; otherwise <c>false</c>.
        /// </returns>
        protected bool SetFirebaseProperty<T>(
            T value,
            [CallerMemberName] string propertyName = null)
        {
            return SetFirebasePropertyWithKey(value, propertyName, propertyName);
        }

        /// <summary>
        /// Sets a property for the firebase object.
        /// </summary>
        /// <typeparam name="T">
        /// The underlying type of the property to get.
        /// </typeparam>
        /// <param name="defaultValue">
        /// The default value of the property to set and return if the property is empty.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property.
        /// </param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        protected T GetFirebaseProperty<T>(
            T defaultValue = default,
            [CallerMemberName] string propertyName = null)
        {
            return GetFirebasePropertyWithKey(propertyName, defaultValue, propertyName);
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
        protected virtual void OnWireError(WireException args)
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

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (!string.IsNullOrEmpty(e.Path))
            {
                var separated = Utils.UrlSeparate(e.Path);
                var key = separated[0];
                lock (this)
                {
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
            }
        }

        private void RealtimeInstance_Error(object sender, WireException e)
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
