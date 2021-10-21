using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Serializers;
using RestfulFirebase.Utilities;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable dictionary.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the dictionary item value.
    /// </typeparam>
    public class FirebaseDictionary<T> : ObservableDictionary<string, T>, IRealtimeModel
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

        private bool isCascadeRealtimeItems;

        #endregion

        #region Initializer

        /// <summary>
        /// Creates new instance of <see cref="FirebaseDictionary{T}"/> class.
        /// </summary>
        /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
        /// Throws when cascade <see cref="IRealtimeModel"/> type <typeparamref name="T"/> has not provided with item initializer and no parameterless constructor.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws when <typeparamref name="T"/> has no supported serializer.
        /// </exception>
        public FirebaseDictionary()
        {
            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new DatabaseInvalidCascadeRealtimeModelException();
                }
                isCascadeRealtimeItems = true;
            }
            else
            {
                if (!Serializer.CanSerialize<T>())
                {
                    throw new SerializerNotSupportedException(typeof(T));
                }
                isCascadeRealtimeItems = false;
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="FirebaseDictionary{T}"/> class.
        /// </summary>
        /// <param name="itemInitializer">
        /// A function item initializer for each item added from the firebase. The function passes the key of the object and returns the <typeparamref name="T"/> item object.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Throws when <paramref name="itemInitializer"/> is null.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws when <typeparamref name="T"/> has no supported serializer.
        /// </exception>
        public FirebaseDictionary(Func<string, T> itemInitializer)
        {
            if (itemInitializer == null)
            {
                throw new ArgumentNullException(nameof(itemInitializer));
            }
            this.itemInitializer = itemInitializer;
            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                isCascadeRealtimeItems = true;
            }
            else
            {
                if (!Serializer.CanSerialize<T>())
                {
                    throw new SerializerNotSupportedException(typeof(T));
                }
                isCascadeRealtimeItems = false;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            await AttachRealtimeAsync(realtimeInstance, invokeSetFirst);
        }

        /// <inheritdoc/>
        public async Task AttachRealtimeAsync(RealtimeInstance realtimeInstance, bool invokeSetFirst)
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

                List<KeyValuePair<string, T>> objs = this.ToList();
                List<string> supPaths = new List<string>();
                foreach (var path in RealtimeInstance.GetSubPaths())
                {
                    var separatedPath = UrlUtilities.Separate(path);
                    var key = separatedPath[0];
                    if (!supPaths.Contains(key)) supPaths.Add(key);
                }

                foreach (var obj in objs)
                {
                    tasks.Add(WireValueAsync(obj.Key, obj.Value, invokeSetFirst));
                    supPaths.RemoveAll(i => i == obj.Key);
                }

                foreach (var path in supPaths)
                {
                    if (this.ContainsKey(path))
                    {
                        continue;
                    }
                    T item = default;
                    if (isCascadeRealtimeItems)
                    {
                        item = ObjectFactory(path);
                        if (item == null)
                        {
                            continue;
                        }
                        tasks.Add(WireValueAsync(path, item, false));
                        TryAdd(path, item);
                    }
                    else
                    {
                        item = Serializer.Deserialize<T>(RealtimeInstance.GetBlob(path));
                        if (item == null)
                        {
                            return;
                        }
                        AddOrUpdate(path, item);
                    }
                }

                await Task.WhenAll(tasks);
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

            if (isCascadeRealtimeItems)
            {
                foreach (var item in this.ToList())
                {
                    (item.Value as IRealtimeModel).DetachRealtime();
                }
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        /// <inheritdoc/>
        public void LoadFromSerializedValue(string serialized, params int[] encryptionPattern)
        {
            if (IsDisposed)
            {
                return;
            }

            var decrypted = Cryptography.VigenereCipherDecrypt(serialized, encryptionPattern);

            Dictionary<string, string> values = BlobUtilities.Convert(decrypted);

            if (values == null)
            {
                return;
            }

            if (isCascadeRealtimeItems)
            {
                foreach (KeyValuePair<string, string> data in values)
                {
                    AddOrUpdate(data.Key, key =>
                    {
                        T value = ObjectFactory(data.Key);
                        (value as IRealtimeModel).LoadFromSerializedValue(data.Value);
                        return value;
                    }, args =>
                    {
                        (args.oldValue as IRealtimeModel).LoadFromSerializedValue(data.Value);
                        return args.oldValue;
                    });
                }
            }
            else
            {
                foreach (KeyValuePair<string, string> data in values)
                {
                    AddOrUpdate(data.Key, Serializer.Deserialize<T>(data.Value));
                }
            }
        }

        /// <inheritdoc/>
        public string GenerateSerializedValue(params int[] encryptionPattern)
        {
            if (IsDisposed)
            {
                return default;
            }

            Dictionary<string, string> values = new Dictionary<string, string>();
            if (isCascadeRealtimeItems)
            {
                foreach (KeyValuePair<string, T> value in this)
                {
                    values.Add(value.Key, (value.Value as IRealtimeModel).GenerateSerializedValue());
                }
            }
            else
            {
                foreach (KeyValuePair<string, T> value in this)
                {
                    values.Add(value.Key, Serializer.Serialize(value.Value));
                }
            }

            string serialized = BlobUtilities.Convert(values);

            return Cryptography.VigenereCipherEncrypt(serialized, encryptionPattern);
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
        protected async void WireValue(string key, T value, bool invokeSetFirst)
        {
            await WireValueAsync(key, value, invokeSetFirst).ConfigureAwait(false);
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
        /// <return>
        /// A <see cref="Task"/> that represents the wire value operation.
        /// </return>
        protected async Task WireValueAsync(string key, T value, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            if (value is IRealtimeModel model)
            {
                model.SyncOperation.SetContext(this);

                if (invokeSetFirst)
                {
                    await RealtimeInstance.Child(key, false).PutModelAsync(model);
                }
                else
                {
                    await RealtimeInstance.Child(key, false).SubModelAsync(model);
                }
            }
            else
            {
                void setBlob()
                {
                    Task.Run(delegate
                    {
                        RealtimeInstance.Child(key, false).SetBlob(Serializer.Serialize(value));
                    }).ConfigureAwait(false);
                }
                setBlob();
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
        protected override void PreAddItems(IEnumerable<KeyValuePair<string, T>> items)
        {
            if (HasAttachedRealtime)
            {
                foreach (var item in items)
                {
                    Task.Run(delegate
                    {
                        WireValue(item.Key, item.Value, true);
                    }).ConfigureAwait(false);
                }
            }
            base.PreAddItems(items);
        }

        /// <inheritdoc/>
        protected override void PreUpdateItems(IEnumerable<KeyValuePair<string, T>> items)
        {
            PreAddItems(items);
            base.PreUpdateItems(items);
        }

        /// <inheritdoc/>
        protected override void PreRemoveItems(IEnumerable<KeyValuePair<string, T>> items)
        {
            foreach (var item in items)
            {
                if (TryGetValue(item.Key, out T value))
                {
                    if (value is IRealtimeModel model)
                    {
                        if (!model.IsNull())
                        {
                            model.SetNull();
                        }
                        model.Dispose();
                    }
                    else
                    {
                        RealtimeInstance?.SetBlob(null, item.Key);
                    }
                }
            }
            base.PreRemoveItems(items);
        }

        /// <inheritdoc/>
        protected override void PreClearItems()
        {
            PreRemoveItems(this);
            base.PreClearItems();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
                if (isCascadeRealtimeItems)
                {
                    foreach (var item in this.ToList())
                    {
                        Remove(item.Key);
                        (item.Value as IRealtimeModel).Dispose();
                    }
                }
                else
                {
                    foreach (var item in this.ToList())
                    {
                        Remove(item.Key);
                    }
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
                var separated = UrlUtilities.Separate(e.Path);
                var key = separated[0];

                try
                {
                    await attachLock.WaitAsync().ConfigureAwait(false);

                    KeyValuePair<string, T> obj = this.FirstOrDefault(i => i.Key == key);
                    if (RealtimeInstance.HasChild(key))
                    {
                        T item = default;
                        if (isCascadeRealtimeItems)
                        {
                            if (obj.Value != null)
                            {
                                return;
                            }
                            item = ObjectFactory(key);
                            if (item == null)
                            {
                                return;
                            }
                            WireValue(key, item, false);
                            TryAdd(key, item);
                        }
                        else
                        {
                            item = Serializer.Deserialize<T>(RealtimeInstance.GetBlob(key));
                            if (item == null)
                            {
                                return;
                            }
                            AddOrUpdate(key, item);
                        }
                    }
                    else if (obj.Value != null)
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
