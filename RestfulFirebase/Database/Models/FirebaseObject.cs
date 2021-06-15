﻿using ObservableHelpers;
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

        public bool HasAttachedRealtime { get => !(RealtimeInstance?.IsDisposed ?? true); }

        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        public event EventHandler<WireErrorEventArgs> WireError;

        #endregion

        #region Methods

        public void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            RealtimeInstance = realtimeInstance;

            Subscribe();

            lock (this)
            {
                InitializeProperties();

                IEnumerable<NamedProperty> props = GetRawProperties(nameof(FirebaseObject));
                List<string> supPaths = new List<string>();
                foreach (var path in RealtimeInstance.GetSubPaths())
                {
                    var separatedPath = Utils.UrlSeparate(path);
                    supPaths.Add(separatedPath[0]);
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
                    var namedProperty = MakeNamedProperty(path, null, nameof(FirebaseObject));
                    RealtimeInstance.Child(path).SubModel((FirebaseProperty)namedProperty.Property);
                    AddCore(namedProperty);
                }
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public void DetachRealtime()
        {
            VerifyNotDisposed();

            Unsubscribe();
            var args = new RealtimeInstanceEventArgs(RealtimeInstance);
            RealtimeInstance = null;

            OnRealtimeDetached(args);
        }

        public void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null)
        {
            VerifyNotDisposed();

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (!(value is IRealtimeModel))
                {
                    throw new Exception("Cascade IRealtimeModel cannot be null. Use IRealtimeModel.SetNull() instead.");
                }
            }

            base.SetPropertyWithKey(value, key, propertyName, nameof(FirebaseObject));
        }

        public T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null)
        {
            VerifyNotDisposed();

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (!(defaultValue is IRealtimeModel))
                {
                    throw new Exception("Cascade IRealtimeModel should have default value");
                }
            }

            return base.GetPropertyWithKey(key, defaultValue, propertyName, nameof(FirebaseObject));
        }

        protected override NamedProperty NamedPropertyFactory(string key, string propertyName, string group)
        {
            VerifyNotDisposed();

            return new NamedProperty()
            {
                Property = new FirebaseProperty(),
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
                foreach (var item in GetRawProperties())
                {
                    if (item.Property is IDisposable model)
                    {
                        model.Dispose();
                    }
                    RemoveCore(item.Key, null);
                }
            }
            base.Dispose(disposing);
        }

        protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            ContextPost(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        protected virtual void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            ContextPost(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
        }

        protected virtual void OnWireError(WireErrorEventArgs args)
        {
            ContextPost(delegate
            {
                WireError?.Invoke(this, args);
            });
        }

        private void Subscribe()
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
                RealtimeInstance.Disposing += RealtimeInstance_Disposing;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
                RealtimeInstance.Disposing -= RealtimeInstance_Disposing;
            }
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            VerifyNotDisposed();

            if (!string.IsNullOrEmpty(e.Path))
            {
                var separated = Utils.UrlSeparate(e.Path);
                var key = separated[0];
                lock (this)
                {
                    NamedProperty namedProperty = GetCore(key, null);
                    if (namedProperty == null && RealtimeInstance.HasChild(key))
                    {
                        namedProperty = MakeNamedProperty(key, null, nameof(FirebaseObject));
                        RealtimeInstance.Child(key).SubModel((FirebaseProperty)namedProperty.Property);
                        AddCore(namedProperty);
                    }
                }
            }
        }

        private void RealtimeInstance_Error(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            OnWireError(e);
        }

        private void RealtimeInstance_Disposing(object sender, EventArgs e)
        {
            Dispose();
        }

        #endregion
    }
}
