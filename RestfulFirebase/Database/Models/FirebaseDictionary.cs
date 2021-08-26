using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
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
        public event EventHandler<WireExceptionEventArgs> WireError;

        private Func<string, T> itemInitializer;

        private SemaphoreSlim attachLock = new SemaphoreSlim(1, 1);

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

            foreach (var item in this.ToList())
            {
                item.Value.DetachRealtime();
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

            Dictionary<string, string> values = Utils.BlobConvert(serialized);

            if (values == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> data in values)
            {
                if (TryGetValueCore(data.Key, out T value))
                {
                    value.LoadFromSerializedValue(data.Value);
                }
                else
                {
                    value = ObjectFactory(data.Key);
                    value.LoadFromSerializedValue(data.Value);
                    Add(data.Key, value);
                }
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

            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (KeyValuePair<string, T> value in this)
            {
                values.Add(value.Key, value.Value.GenerateSerializedValue());
            }

            return Utils.BlobConvert(values);
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

            if (invokeSetFirst)
            {
                RealtimeInstance.Child(key).PutModel(value);
            }
            else
            {
                RealtimeInstance.Child(key).SubModel(value);
            }
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
                    Task.Run(delegate
                    {
                        WireValue(key, value, true);
                    }).ConfigureAwait(false);
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
                    if (!value.IsNull())
                    {
                        value.SetNull();
                    }
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
