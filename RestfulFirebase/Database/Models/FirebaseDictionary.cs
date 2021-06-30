﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Extensions;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable dictionary.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the dictionary item value.
    /// </typeparam>
    public class FirebaseDictionary<T> : ObservableDictionary<string, T>, IRealtimeModel
        where T : IRealtimeModel
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

        private Func<string, T> itemInitializer;

        #endregion

        #region Initializer

        /// <summary>
        /// Creates new instance of <see cref="FirebaseDictionary{T}"/> class.
        /// </summary>
        public FirebaseDictionary()
        {
            if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
            {
                throw new Exception("FirebaseDictionary item with no parameterless constructor should have an item initializer.");
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="FirebaseDictionary{T}"/> class.
        /// </summary>
        /// <param name="itemInitializer">
        /// A function item initializer for each item added from the firebase. The function passes the key of the object and returns the <typeparamref name="T"/> item object.
        /// </param>
        public FirebaseDictionary(Func<string, T> itemInitializer)
        {
            this.itemInitializer = itemInitializer;
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

                List<KeyValuePair<string, T>> objs = this.ToList();
                List<string> supPaths = new List<string>();
                foreach (var path in RealtimeInstance.GetSubPaths())
                {
                    var separatedPath = Utils.UrlSeparate(path);
                    var key = separatedPath[0];
                    if (!supPaths.Contains(key)) supPaths.Add(key);
                }

                foreach (var obj in objs)
                {
                    WireValue(obj.Key, obj.Value, invokeSetFirst);
                    supPaths.RemoveAll(i => i == obj.Key);
                }

                foreach (var path in supPaths)
                {
                    if (this.Any(i => i.Key == path)) continue;
                    var item = ObjectFactory(path);
                    if (item == null) continue;
                    WireValue(path, item, false);
                    if (TryAddCore(path, item))
                    {
                        NotifyObserversOfChange();
                    }
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

            foreach (var item in this.ToList())
            {
                item.Value.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        /// <summary>
        /// Factory used for creating the item object.
        /// </summary>
        /// <param name="key">
        /// The key of the <typeparamref name="T"/> item object.
        /// </param>
        /// <returns>
        /// The created <typeparamref name="T"/> item object.
        /// </returns>
        protected T ObjectFactory(string key)
        {
            if (IsDisposed)
            {
                return default;
            }

            if (itemInitializer == null)
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
            else
            {
                return itemInitializer.Invoke((key));
            }
        }

        /// <summary>
        /// Wires the provided <paramref name="value"/> to the realtime instance of the model.
        /// </summary>
        /// <param name="key">
        /// The key of the <typeparamref name="T"/> item object to wire.
        /// </param>
        /// <param name="value">
        /// The value to wire.
        /// </param>
        /// <param name="invokeSetFirst">
        /// Specify <c>true</c> whether the value should be put and subscribe to the realtime instance; otherwise <c>false</c> to only subscribe to the realtime instance.
        /// </param>
        protected void WireValue(string key, T value, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            value.SyncOperation.SetContext(this);

            if (invokeSetFirst) RealtimeInstance.Child(key).PutModel(value);
            else RealtimeInstance.Child(key).SubModel(value);
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

        /// <inheritdoc/>
        protected override bool ValidateSetItem(string key, T value)
        {
            if (IsDisposed)
            {
                return false;
            }

            var baseValidation = base.ValidateSetItem(key, value);

            if (baseValidation)
            {
                if (HasAttachedRealtime)
                {
                    WireValue(key, value, true);
                }
            }

            return baseValidation;
        }

        /// <inheritdoc/>
        protected override bool ValidateRemoveItem(string key)
        {
            if (IsDisposed)
            {
                return false;
            }

            var baseValidation = base.ValidateRemoveItem(key);

            if (baseValidation)
            {
                if (TryGetValueCore(key, out T value))
                {
                    if (!value.IsNull()) value.SetNull();
                    value.Dispose();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return baseValidation;
        }

        /// <inheritdoc/>
        protected override bool ValidateClear()
        {
            if (IsDisposed)
            {
                return false;
            }

            var baseValidation = base.ValidateClear();

            if (baseValidation)
            {
                foreach (var item in this.ToList())
                {
                    ValidateRemoveItem(item.Key);
                }
            }

            return baseValidation;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
                foreach (var item in this.ToList())
                {
                    TryRemoveWithNotification(item.Key, out _);
                    item.Value.Dispose();
                }
            }
            base.Dispose(disposing);
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
                    KeyValuePair<string, T> obj = this.FirstOrDefault(i => i.Key == key);
                    var hasChild = RealtimeInstance.HasChild(key);
                    if (obj.Value == null && hasChild)
                    {
                        var item = ObjectFactory(key);
                        if (item == null)
                        {
                            return;
                        }
                        WireValue(key, item, false);
                        if (TryAddCore(key, item))
                        {
                            NotifyObserversOfChange();
                        }
                    }
                    else if (obj.Value != null && !hasChild)
                    {
                        Remove(key);
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
