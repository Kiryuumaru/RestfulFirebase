using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Local;
using RestfulFirebase.Serializers;
using RestfulFirebase.Utilities;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model <see cref="ObservableDictionary{TKey, TValue}"/> for the <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the dictionary item value.
    /// </typeparam>
    public class FirebaseDictionary<T> : ObservableDictionary<string, T>, IInternalRealtimeModel
    {
        #region Properties

        private Func<string, T> itemInitializer;

        private SemaphoreSlim attachLock = new SemaphoreSlim(1, 1);

        private bool isCascadeRealtimeItems;

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseDictionary{T}"/> class.
        /// </summary>
        /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
        /// Throws when cascade <see cref="IInternalRealtimeModel"/> type <typeparamref name="T"/> has not provided with item initializer and no parameterless constructor.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws when <typeparamref name="T"/> has no supported serializer.
        /// </exception>
        public FirebaseDictionary()
        {
            if (typeof(IInternalRealtimeModel).IsAssignableFrom(typeof(T)))
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
            if (typeof(IInternalRealtimeModel).IsAssignableFrom(typeof(T)))
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
                return Activator.CreateInstance<T>();
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

            if (value is IInternalRealtimeModel model)
            {
                model.SyncOperation.SetContext(this);

                if (invokeSetFirst)
                {
                    await RealtimeInstance.Child(key).PutModelAsync(model);
                }
                else
                {
                    await RealtimeInstance.Child(key).SubModelAsync(model);
                }
            }
            else
            {
                RealtimeInstance.Child(key).SetValue(Serializer.Serialize(value));
            }
        }

        #endregion

        #region ObservableCollection<T> Members

        /// <inheritdoc/>
        protected override bool InternalClearItems(out int lastCount)
        {
            if (base.InternalClearItems(out lastCount))
            {
                Task.Run(() => RemoveFromWire(this.ToList()));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool InternalInsertItems(int index, IEnumerable<KeyValuePair<string, T>> items, out int lastCount)
        {
            if (base.InternalInsertItems(index, items, out lastCount))
            {
                Task.Run(() => AddToWire(items));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool InternalRemoveItems(int index, int count, out IEnumerable<KeyValuePair<string, T>> oldItems)
        {
            if (base.InternalRemoveItems(index, count, out oldItems))
            {
                IEnumerable<KeyValuePair<string, T>> proxy = oldItems;
                Task.Run(() => RemoveFromWire(proxy));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool InternalSetItem(int index, KeyValuePair<string, T> item, out KeyValuePair<string, T> originalItem)
        {
            if (base.InternalSetItem(index, item, out originalItem))
            {
                Task.Run(() => AddToWire(new KeyValuePair<string, T>[] { item }));
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddToWire(IEnumerable<KeyValuePair<string, T>> items)
        {
            if (HasAttachedRealtime)
            {
                foreach (var item in items)
                {
                    WireValue(item.Key, item.Value, true);
                }
            }
        }

        private void RemoveFromWire(IEnumerable<KeyValuePair<string, T>> items)
        {
            if (HasAttachedRealtime)
            {
                foreach (var item in items)
                {
                    if (item.Value is IInternalRealtimeModel model)
                    {
                        if (!model.IsNull())
                        {
                            model.SetNull();
                        }
                        model.Dispose();
                    }
                    else
                    {
                        RealtimeInstance?.SetValue(null, item.Key);
                    }
                }
            }
        }

        #endregion

        #region Disposable Members

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (isCascadeRealtimeItems)
                {
                    foreach (var item in this)
                    {
                        (item.Value as IInternalRealtimeModel).Dispose();
                    }
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

                foreach (var obj in this)
                {
                    tasks.Add(WireValueAsync(obj.Key, obj.Value, invokeSetFirst));
                    subPaths.RemoveAll(i => i == obj.Key);
                }

                foreach (var path in subPaths)
                {
                    if (isCascadeRealtimeItems)
                    {
                        TryAdd(path, _ =>
                        {
                            T item = ObjectFactory(path);
                            tasks.Add(WireValueAsync(path, item, false));
                            return item;
                        });
                    }
                    else
                    {
                        AddOrUpdate(path, _ => Serializer.Deserialize<T>(RealtimeInstance.GetValue(path)));
                    }
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

            if (isCascadeRealtimeItems)
            {
                foreach (var item in this.ToList())
                {
                    (item.Value as IInternalRealtimeModel).DetachRealtime();
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

            //    KeyValuePair<string, T> obj = this.FirstOrDefault(i => i.Key == key);
            //    if (RealtimeInstance.HasChild(key))
            //    {
            //        if (isCascadeRealtimeItems)
            //        {
            //            if (obj.Value != null)
            //            {
            //                return;
            //            }
            //            TryAdd(key, _ =>
            //            {
            //                T item = ObjectFactory(key);
            //                WireValue(key, ObjectFactory(key), false);
            //                return item;
            //            });
            //        }
            //        else
            //        {
            //            AddOrUpdate(key, _ => Serializer.Deserialize<T>(RealtimeInstance.GetBlob(key)));
            //        }
            //    }
            //    else if (obj.Value != null)
            //    {
            //        Remove(key);
            //    }
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
