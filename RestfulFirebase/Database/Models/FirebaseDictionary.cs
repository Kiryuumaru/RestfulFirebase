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
        private bool isCascadeRealtimeItems;
        private bool? isInvokeToSetFirst;
        private bool hasPostAttachedRealtime;

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

        /// <summary>
        /// Creates new instance of the <see cref="FirebaseDictionary{T}"/> class that contains elements copied from the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">
        /// The initial items of the dictionary.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
        /// Throws when cascade <see cref="IInternalRealtimeModel"/> type <typeparamref name="T"/> has not provided with item initializer and no parameterless constructor.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws when <typeparamref name="T"/> has no supported serializer.
        /// </exception>
        public FirebaseDictionary(IEnumerable<KeyValuePair<string, T>> items)
            : base(items)
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
        /// Creates new instance of the <see cref="FirebaseDictionary{T}"/> class that contains elements copied from the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">
        /// The initial items of the dictionary.
        /// </param>
        /// <param name="itemInitializer">
        /// A function item initializer for each item added from the firebase. The function passes the key of the object and returns the <typeparamref name="T"/> item object.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemInitializer"/> or <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="DatabaseInvalidCascadeRealtimeModelException">
        /// Throws when cascade <see cref="IInternalRealtimeModel"/> type <typeparamref name="T"/> has not provided with item initializer and no parameterless constructor.
        /// </exception>
        /// <exception cref="SerializerNotSupportedException">
        /// Throws when <typeparamref name="T"/> has no supported serializer.
        /// </exception>
        public FirebaseDictionary(Func<string, T> itemInitializer, IEnumerable<KeyValuePair<string, T>> items)
            : base(items)
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
        /// <param name="instance">
        /// The instance used to wire the value.
        /// </param>
        /// <param name="key">
        /// The key of the <typeparamref name="T"/> item object to wire.
        /// </param>
        /// <param name="value">
        /// The value to wire.
        /// </param>
        /// <param name="invokeSetFirst">
        /// Specify <c>true</c> whether the value should be put and subscribe to the realtime instance; otherwise <c>false</c> to only subscribe to the realtime instance.
        /// </param>
        protected void WireValue(RealtimeInstance instance, string key, T value, bool invokeSetFirst)
        {
            if (value is IInternalRealtimeModel model)
            {
                void wireModel()
                {
                    RealtimeInstance modelRealtimeInstance = model.RealtimeInstance;
                    if (modelRealtimeInstance != null)
                    {
                        if (!modelRealtimeInstance.IsDisposed && instance.IsSubPath(modelRealtimeInstance))
                        {
                            return;
                        }
                    }

                    model.SyncOperation.SetContext(this);

                    if (invokeSetFirst)
                    {
                        instance.Child(key).PutModel(model);
                    }
                    else
                    {
                        instance.Child(key).SubModel(model);
                    }
                }
                if (model.HasPostAttachedRealtime)
                {
                    Task.Run(async () =>
                    {
                        while (model.HasPostAttachedRealtime)
                        {
                            await Task.Delay(1000);
                        }

                        wireModel();
                    });
                }
                else
                {
                    wireModel();
                }
            }
            else
            {
                instance.Child(key).SetValue(Serializer.Serialize(value));
            }
        }

        #endregion

        #region ObservableCollection<T> Members

        /// <inheritdoc/>
        protected override bool InternalClearItems(out IEnumerable<KeyValuePair<string, T>> oldItems)
        {
            if (base.InternalClearItems(out oldItems))
            {
                RemoveFromWire(oldItems);
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
                AddToWire(items);
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
                RemoveFromWire(oldItems);
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
                AddToWire(new KeyValuePair<string, T>[] { item });
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddToWire(IEnumerable<KeyValuePair<string, T>> items)
        {
            RealtimeInstance instance = RealtimeInstance;
            if (instance != null && !instance.IsDisposed)
            {
                foreach (var item in items)
                {
                    WireValue(instance, item.Key, item.Value, true);
                }
            }
        }

        private void RemoveFromWire(IEnumerable<KeyValuePair<string, T>> items)
        {
            RealtimeInstance instance = RealtimeInstance;
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
                else if (instance != null)
                {
                    instance.SetValue(null, item.Key);
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

        bool? IInternalRealtimeModel.IsInvokeToSetFirst => isInvokeToSetFirst;

        bool IInternalRealtimeModel.HasPostAttachedRealtime => hasPostAttachedRealtime;

        void IInternalRealtimeModel.AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (IsDisposed && !realtimeInstance.IsDisposed)
            {
                return;
            }

            hasPostAttachedRealtime = true;

            RWLock.LockWriteAndForget(() =>
            {
                try
                {
                    if (IsDisposed && !realtimeInstance.IsDisposed)
                    {
                        return;
                    }

                    List<string> children = realtimeInstance
                        .GetChildren()
                        .Select(i => i.key)
                        .ToList();

                    foreach (var obj in this)
                    {
                        WireValue(realtimeInstance, obj.Key, obj.Value, invokeSetFirst);
                        children.Remove(obj.Key);
                    }

                    foreach (var path in children)
                    {
                        if (isCascadeRealtimeItems)
                        {
                            if (!ContainsKey(path))
                            {
                                T item = ObjectFactory(path);
                                if (item != null)
                                {
                                    WireValue(realtimeInstance, path, item, false);
                                    AddOrUpdate(path, item);
                                }
                            }
                        }
                        else
                        {
                            AddOrUpdate(path, _ => Serializer.Deserialize<T>(realtimeInstance.GetValue(path)));
                        }
                    }

                    Subscribe(realtimeInstance, invokeSetFirst);

                    RWLock.InvokeOnLockExit(() => OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance)));
                }
                catch
                {
                    Unsubscribe();

                    throw;
                }
                finally
                {
                    hasPostAttachedRealtime = false;
                }
            });
        }

        void IInternalRealtimeModel.DetachRealtime()
        {
            if (IsDisposed || !HasAttachedRealtime)
            {
                return;
            }

            RWLock.LockWrite(() =>
            {
                if (isCascadeRealtimeItems)
                {
                    foreach (var item in this.ToList())
                    {
                        (item.Value as IInternalRealtimeModel).DetachRealtime();
                    }
                }

                var args = new RealtimeInstanceEventArgs(RealtimeInstance);

                Unsubscribe();

                RWLock.InvokeOnLockExit(() => OnRealtimeDetached(args));
            });
        }

        private void Subscribe(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (IsDisposed || realtimeInstance.IsDisposed)
            {
                return;
            }

            RWLock.LockWrite(() =>
            {
                if (IsDisposed || realtimeInstance.IsDisposed)
                {
                    return;
                }

                if (HasAttachedRealtime)
                {
                    Unsubscribe();
                }

                RealtimeInstance = realtimeInstance;
                isInvokeToSetFirst = invokeSetFirst;

                RealtimeInstance.ImmediateDataChanges += RealtimeInstance_ImmediateDataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
                RealtimeInstance.Disposing += RealtimeInstance_Disposing;
            });
        }

        private void Unsubscribe()
        {
            if (IsDisposed)
            {
                return;
            }

            if (!HasAttachedRealtime)
            {
                return;
            }

            RWLock.LockWrite(() =>
            {
                if (IsDisposed)
                {
                    return;
                }

                if (!HasAttachedRealtime)
                {
                    return;
                }

                RealtimeInstance.ImmediateDataChanges -= RealtimeInstance_ImmediateDataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
                RealtimeInstance.Disposing -= RealtimeInstance_Disposing;

                RealtimeInstance = null;
                isInvokeToSetFirst = null;
            });
        }

        private void RealtimeInstance_ImmediateDataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (e.Path.Length == 0)
            {
                return;
            }

            RealtimeInstance instance = RealtimeInstance;
            if (instance == null || instance.IsDisposed)
            {
                return;
            }

            RWLock.LockWrite(() =>
            {
                if (isCascadeRealtimeItems)
                {
                    var data1 = instance.InternalGetRecursiveData(e.Path);
                    var data = instance.InternalGetRecursiveData(e.Path[0]);
                    if (data.Length == 0)
                    {
                        Remove(e.Path[0]);
                    }
                    else
                    {
                        if (data.All(i => i.changesType == LocalDataChangesType.Delete))
                        {
                            Remove(e.Path[0]);
                        }
                        else
                        {
                            if (!ContainsKey(e.Path[0]))
                            {
                                T item = ObjectFactory(e.Path[0]);
                                if (item != null)
                                {
                                    WireValue(instance, e.Path[0], item, false);
                                    AddOrUpdate(e.Path[0], item);
                                }
                            }
                        }
                    }
                }
                else
                {
                    instance.InternalGetDataOrChildren(
                        () =>
                        {
                            Remove(e.Path[0]);
                        },
                        data =>
                        {
                            if (data.changesType == LocalDataChangesType.Delete)
                            {
                                Remove(e.Path[0]);
                            }
                            else if (this.TryGetValue(e.Path[0], out T value))
                            {
                                T newValue = Serializer.Deserialize<T>(data.value);
                                if (!EqualityComparer<T>.Default.Equals(value, newValue))
                                {
                                    AddOrUpdate(e.Path[0], _ => Serializer.Deserialize<T>(data.value));
                                }
                            }
                            else
                            {
                                AddOrUpdate(e.Path[0], _ => Serializer.Deserialize<T>(data.value));
                            }
                        },
                        onPath =>
                        {
                            if (this.ContainsKey(e.Path[0]))
                            {
                                Remove(e.Path[0]);
                            }
                        },
                        e.Path[0]);
                }
            });
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
            ContextPost(delegate
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
            ContextPost(delegate
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
