using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Common.Observables
{
    public class ObservableDictionary<TKey, TValue> :
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        INotifyCollectionChanged, IObservable
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private SynchronizationContext Context
        {
            get => Holder.GetAttribute<SynchronizationContext>(AsyncOperationManager.SynchronizationContext);
            set => Holder.SetAttribute(value);
        }

        private ConcurrentDictionary<TKey, TValue> Dictionary
        {
            get => Holder.GetAttribute<ConcurrentDictionary<TKey, TValue>>(new ConcurrentDictionary<TKey, TValue>());
            set => Holder.SetAttribute(value);
        }

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => Holder.GetAttribute<PropertyChangedEventHandler>(delegate { });
            set => Holder.SetAttribute(value);
        }

        private NotifyCollectionChangedEventHandler CollectionChangedHandler
        {
            get => Holder.GetAttribute<NotifyCollectionChangedEventHandler>(delegate { });
            set => Holder.SetAttribute(value);
        }

        private EventHandler<ContinueExceptionEventArgs> PropertyErrorHandler
        {
            get => Holder.GetAttribute<EventHandler<ContinueExceptionEventArgs>>(delegate { });
            set => Holder.SetAttribute(value);
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    PropertyChangedHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    PropertyChangedHandler -= value;
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                lock (this)
                {
                    CollectionChangedHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    CollectionChangedHandler -= value;
                }
            }
        }

        public event EventHandler<ContinueExceptionEventArgs> PropertyError
        {
            add
            {
                lock (this)
                {
                    PropertyErrorHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    PropertyErrorHandler -= value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get => Dictionary.Keys;
        }

        public ICollection<TValue> Values
        {
            get => Dictionary.Values;
        }

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set => UpdateWithNotification(key, value);
        }

        #endregion

        #region Initializers

        public ObservableDictionary(IAttributed attributed)
        {
            Holder.Inherit(attributed);
        }

        public ObservableDictionary()
        {
            Holder.Inherit(null);
        }

        #endregion

        #region Methods

        private void NotifyObserversOfChange()
        {
            var collectionHandler = CollectionChangedHandler;
            var propertyHandler = PropertyChangedHandler;
            void invoke()
            {
                if (collectionHandler != null)
                {
                    collectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
                if (propertyHandler != null)
                {
                    propertyHandler(this, new PropertyChangedEventArgs("Count"));
                    propertyHandler(this, new PropertyChangedEventArgs("Keys"));
                    propertyHandler(this, new PropertyChangedEventArgs("Values"));
                }
            }
            if (collectionHandler != null || propertyHandler != null)
            {
                invoke();
                //Context.Post(s => invoke(), null);
            }
        }

        private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            return TryAddWithNotification(item.Key, item.Value);
        }

        private bool TryAddWithNotification(TKey key, TValue value)
        {
            bool result = Dictionary.TryAdd(key, ValueFactory(key, value).value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        private bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            bool result = Dictionary.TryRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        private void UpdateWithNotification(TKey key, TValue value)
        {
            Dictionary[key] = ValueFactory(key, value).value;
            NotifyObserversOfChange();
        }

        protected virtual (TKey key, TValue value) ValueFactory(TKey key, TValue value)
        {
            return (key, value);
        }

        public virtual void OnError(Exception exception, bool defaultIgnoreAndContinue = true)
        {
            var args = new ContinueExceptionEventArgs(exception, defaultIgnoreAndContinue);
            PropertyErrorHandler?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public virtual void OnError(ContinueExceptionEventArgs args)
        {
            PropertyErrorHandler?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public void Add(TKey key, TValue value)
        {
            TryAddWithNotification(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            TValue temp;
            return TryRemoveWithNotification(key, out temp);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            TryAddWithNotification(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Clear();
            NotifyObserversOfChange();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).IsReadOnly; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue temp;
            return TryRemoveWithNotification(item.Key, out temp);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).GetEnumerator();
        }

        #endregion
    }
}
