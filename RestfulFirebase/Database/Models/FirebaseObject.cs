using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObject : ObservableObject, IRealtimeModelProxy
    {
        #region Properties

        private const string UnwiredBlobTag = "unwired";

        internal RealtimeInstance RealtimeInstance { get; private set; }

        #endregion

        #region Methods

        public void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null)
        {
            VerifyNotDisposed();

            base.SetPropertyWithKey(value, key, propertyName, nameof(FirebaseObject), FirebaseProperty.SerializableTag, validateValue);
        }

        public T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null)
        {
            VerifyNotDisposed();

            return base.GetPropertyWithKey(key, defaultValue, propertyName, nameof(FirebaseObject), FirebaseProperty.SerializableTag);
        }

        public virtual bool SetPersistablePropertiesNull(object parameter = null)
        {
            VerifyNotDisposed();

            var hasChanges = false;
            List<PropertyHolder> props = new List<PropertyHolder>();
            lock (PropertyHolders)
            {
                props = GetRawProperties(nameof(FirebaseObject)).ToList();
            }
            foreach (var propHolder in props)
            {
                if (propHolder.Property.SetNull(parameter)) hasChanges = true;
            }
            return hasChanges;
        }

        public virtual bool IsPersistablePropertiesNull(object parameter = null)
        {
            VerifyNotDisposed();

            List<PropertyHolder> props = new List<PropertyHolder>();
            lock (PropertyHolders)
            {
                props = GetRawProperties(nameof(FirebaseObject)).ToList();
            }
            return props.All(i => i.Property.IsNull(parameter));
        }

        public override bool SetNull(object parameter = null)
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return RealtimeInstance.SetNull();
            }
            else
            {
                return base.SetNull(parameter);
            }
        }

        public override bool IsNull(object parameter = null)
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return RealtimeInstance.IsNull();
            }
            else
            {
                return base.IsNull(parameter);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }
            base.Dispose(disposing);
        }

        protected override PropertyHolder PropertyFactory(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            var prop = new FirebaseProperty();
            var propHolder = new PropertyHolder()
            {
                Property = prop,
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
            prop.PropertyChangedInternal += (s, e) =>
            {
                if (IsDisposedOrDisposing) return;

                if (e.PropertyName == nameof(prop.Property))
                {
                    InvokeOnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            };
            return propHolder;
        }

        private void Subscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.OnInternalChanges += RealtimeInstance_OnInternalChanges;
                RealtimeInstance.OnInternalError += RealtimeInstance_OnInternalError;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.OnInternalChanges -= RealtimeInstance_OnInternalChanges;
                RealtimeInstance.OnInternalError -= RealtimeInstance_OnInternalError;
            }
        }

        private void RealtimeInstance_OnInternalChanges(object sender, DataChangesEventArgs e)
        {
            VerifyNotDisposed();

            if (!string.IsNullOrEmpty(e.Path))
            {
                var separated = Utils.UrlSeparate(e.Path);
                var key = separated[0];
                var dataCount = RealtimeInstance.Child(key).GetTotalDataCount();
                PropertyHolder propHolder = null;
                lock (PropertyHolders)
                {
                    propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
                }
                if (propHolder == null && dataCount != 0)
                {
                    propHolder = PropertyFactory(key, null, nameof(FirebaseObject));
                    RealtimeInstance.Child(key).SubModel((FirebaseProperty)propHolder.Property);
                    lock (PropertyHolders)
                    {
                        PropertyHolders.Add(propHolder);
                    }
                }
                InvokeOnChangedWithKey(key);
            }
        }

        private void RealtimeInstance_OnInternalError(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            InvokeOnError(e.Exception);
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            RealtimeInstance = realtimeInstance;

            Subscribe();

            InitializeProperties();

            List<PropertyHolder> props = new List<PropertyHolder>();
            lock (PropertyHolders)
            {
                props = GetRawProperties(nameof(FirebaseObject)).ToList();
            }
            var paths = RealtimeInstance.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var prop in props)
            {
                if (invokeSetFirst) RealtimeInstance.Child(prop.Key).PutModel((FirebaseProperty)prop.Property);
                else RealtimeInstance.Child(prop.Key).SubModel((FirebaseProperty)prop.Property);
                paths.RemoveAll(i => i == prop.Key);
            }

            foreach (var path in paths)
            {
                lock (PropertyHolders)
                {
                    if (PropertyHolders.Any(i => i.Key == path)) continue;
                }
                var propHolder = PropertyFactory(path, null, nameof(FirebaseObject));
                RealtimeInstance.Child(path).SubModel((FirebaseProperty)propHolder.Property);
                lock (PropertyHolders)
                {
                    PropertyHolders.Add(propHolder);
                }
            }
        }

        #endregion
    }
}
