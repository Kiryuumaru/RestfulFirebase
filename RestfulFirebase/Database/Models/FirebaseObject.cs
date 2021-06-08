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
    public class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

        public RealtimeInstance RealtimeInstance { get; private set; }

        public bool HasAttachedRealtime { get => RealtimeInstance != null; }

        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        public event EventHandler<WireErrorEventArgs> WireError;

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

            if (RealtimeInstance != null && parameter?.ToString() != FirebaseProperty.UnwiredBlobTag)
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

            if (RealtimeInstance != null && parameter?.ToString() != FirebaseProperty.UnwiredBlobTag)
            {
                return RealtimeInstance.IsNull();
            }
            else
            {
                return base.IsNull(parameter);
            }
        }

        public virtual void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
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

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public virtual void DetachRealtime()
        {
            VerifyNotDisposed();

            Unsubscribe();
            var args = new RealtimeInstanceEventArgs(RealtimeInstance);
            RealtimeInstance = null;
            OnRealtimeDetached(args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
            }
            base.Dispose(disposing);
        }

        protected void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        protected void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
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
            prop.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(prop.Property))
                {
                    OnPropertyChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            };
            return propHolder;
        }

        protected virtual void OnWireError(WireErrorEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                WireError?.Invoke(this, args);
            });
        }

        private void Subscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
            }
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
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
                OnPropertyChangedWithKey(key);
            }
        }

        private void RealtimeInstance_Error(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            OnWireError(e);
        }

        #endregion
    }
}
