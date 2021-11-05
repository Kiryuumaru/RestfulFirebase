using System;
using System.Collections.Concurrent;
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
    /// Provides an observable model <see cref="ObservableCollection{T}"/> for the <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The undelying type of the collection item value.
    /// </typeparam>
    public class FirebaseCollection<T> : ObservableCollection<T>, IInternalRealtimeModel
    {
        #region Properties

        private Func<string, T> itemInitializer;

        private SemaphoreSlim attachLock = new SemaphoreSlim(1, 1);

        private bool isCascadeRealtimeItems;

        private readonly List<ValueHolder> valueHolders = new List<ValueHolder>();

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseCollection{T}"/> class.
        /// </summary>
        /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
        /// Throws when cascade <see cref="IInternalRealtimeModel"/> type <typeparamref name="T"/> has not provided with item initializer and no parameterless constructor.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws when <typeparamref name="T"/> has no supported serializer.
        /// </exception>
        public FirebaseCollection()
            : base()
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
        /// Creates new instance of <see cref="FirebaseCollection{T}"/> class.
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
        public FirebaseCollection(Func<string, T> itemInitializer)
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

        private async void WireValue(string key, ValueHolder valueHolder, bool invokeSetFirst)
        {
            await WireValueAsync(key, valueHolder, invokeSetFirst).ConfigureAwait(false);
        }

        private async Task WireValueAsync(string key, ValueHolder valueHolder, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            valueHolder.SyncOperation.SetContext(this);

            if (invokeSetFirst)
            {
                await RealtimeInstance.Child(key).PutModelAsync(valueHolder);
            }
            else
            {
                await RealtimeInstance.Child(key).SubModelAsync(valueHolder);
            }
        }

        #endregion

        #region ObservableCollection<T> Members

        /// <inheritdoc/>
        protected override bool InternalClearItems(out int lastCount)
        {
            if (base.InternalClearItems(out lastCount))
            {
                int currentIndex = 0;
                foreach (var item in this.ToArray())
                {
                    ValueHolder valueHolder = valueHolders[currentIndex];
                    if (!valueHolder.IsNull())
                    {
                        valueHolder.SetNull();
                    }
                    valueHolder.Dispose();
                    currentIndex++;
                }
                valueHolders.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool InternalMoveItem(int oldIndex, int newIndex, out T movedItem)
        {
            if (base.InternalMoveItem(oldIndex, newIndex, out movedItem))
            {
                ValueHolder valueHolder = valueHolders[oldIndex];
                valueHolders.RemoveAt(oldIndex);
                valueHolders.Insert(newIndex, valueHolder);
                valueHolder.Index = newIndex;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool InternalInsertItems(int index, IEnumerable<T> items, out int lastCount)
        {
            if (base.InternalInsertItems(index, items, out lastCount))
            {
                int currentIndex = index;
                foreach (var item in items)
                {
                    ValueHolder valueHolder = new ValueHolder(UIDFactory.GenerateSafeUID(), index, item);
                    valueHolders.Insert(currentIndex, valueHolder);
                    WireValue(valueHolder.Key, valueHolder, true);
                    for (int i = currentIndex + 1; i < valueHolders.Count; i++)
                    {
                        valueHolders[i].Index = i;
                    }
                    currentIndex++;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool InternalRemoveItems(int index, int count, out IEnumerable<T> oldItems)
        {
            if (base.InternalRemoveItems(index, count, out oldItems))
            {
                int currentIndex = index;
                foreach (var item in oldItems)
                {
                    ValueHolder valueHolder = valueHolders[currentIndex];
                    if (!valueHolder.IsNull())
                    {
                        valueHolder.SetNull();
                    }
                    valueHolder.Dispose();
                    valueHolders.RemoveAt(currentIndex);
                    for (int i = currentIndex; i < valueHolders.Count; i++)
                    {
                        valueHolders[i].Index = i;
                    }
                    currentIndex++;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool InternalSetItem(int index, T item, out T originalItem)
        {
            if (base.InternalSetItem(index, item, out originalItem))
            {
                valueHolders[index].Value = item;
                return true;
            }
            else
            {
                return false;
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
                    foreach (var item in this.ToList())
                    {
                        (item as IInternalRealtimeModel).Dispose();
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

                foreach (var obj in valueHolders.ToList())
                {
                    tasks.Add(WireValueAsync(obj.Key, obj, invokeSetFirst));
                    subPaths.RemoveAll(i => i == obj.Key);
                }

                foreach (var path in subPaths)
                {
                    if (isCascadeRealtimeItems)
                    {
                        T item = ObjectFactory(path);
                        Add(ObjectFactory(path));
                    }
                    else
                    {
                        //AddOrUpdate(path, _ => Serializer.Deserialize<T>(RealtimeInstance.GetBlob(path)));
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
                foreach (var item in this)
                {
                    (item as IInternalRealtimeModel).DetachRealtime();
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
            //if (IsDisposed)
            //{
            //    return;
            //}

            //if (string.IsNullOrEmpty(e.Path))
            //{
            //    return;
            //}

            //int keyLastIndex = e.Path.IndexOf('/');
            //string key = keyLastIndex == -1 ? e.Path : e.Path.Substring(0, keyLastIndex);

            //try
            //{
            //    await attachLock.WaitAsync().ConfigureAwait(false);

            //    ValueHolder valueHolder = valueHolders.FirstOrDefault(i => i.Key == key);
            //    if (RealtimeInstance.HasChild(key))
            //    {
            //        if (isCascadeRealtimeItems)
            //        {
            //            if (valueHolder.Value != null)
            //            {
            //                return;
            //            }
            //            T item = ObjectFactory(key);
            //            WireValue(key, item, false);
            //            return item;
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

        #region Helper Classes

        private class ValueHolder : FirebaseObject
        {
            public string Key
            {
                get => GetProperty<string>();
                set => SetProperty(value);
            }

            public int Index
            {
                get => GetFirebasePropertyWithKey<int>("index");
                set => SetFirebasePropertyWithKey(value, "index");
            }

            public T Value
            {
                get => GetFirebasePropertyWithKey<T>("value");
                set => SetFirebasePropertyWithKey(value, "value");
            }

            public ValueHolder(string key, int index, T value)
            {
                Key = key;
                Index = index;
                Value = value;
            }
        }

        #endregion
    }
}
