using ObservableHelpers.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Provides a read-write with key based locker.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key.
    /// </typeparam>
    public class RWLockDictionary<TKey> : RWLock
    {
        #region Properties

        private ConcurrentDictionary<TKey, Lock> locks = new ConcurrentDictionary<TKey, Lock>();

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="RWLockDictionary{TKey}"/>.
        /// </summary>
        public RWLockDictionary()
        {
            locks = new ConcurrentDictionary<TKey, Lock>();
        }

        /// <summary>
        /// Creates new instance of <see cref="RWLockDictionary{TKey}"/>, specifying the <see cref="LockRecursionPolicy"/>.
        /// </summary>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> of the locker.
        /// </param>
        public RWLockDictionary(LockRecursionPolicy recursionPolicy)
            : base(recursionPolicy)
        {
            locks = new ConcurrentDictionary<TKey, Lock>();
        }

        /// <summary>
        /// Creates new instance of <see cref="RWLockDictionary{TKey}"/>, specifying the <see cref="IEqualityComparer{TKey}"/> of the <typeparamref name="TKey"/>.
        /// </summary>
        /// <param name="equalityComparer">
        /// The equality comparison implementation to use when comparing keys.
        /// </param>
        public RWLockDictionary(IEqualityComparer<TKey> equalityComparer)
        {
            locks = new ConcurrentDictionary<TKey, Lock>(equalityComparer);
        }

        /// <summary>
        /// Creates new instance of <see cref="RWLockDictionary{TKey}"/>, specifying the <see cref="LockRecursionPolicy"/> and the <see cref="IEqualityComparer{TKey}"/> of the <typeparamref name="TKey"/>.
        /// </summary>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> of the locker.
        /// </param>
        /// <param name="equalityComparer">
        /// The equality comparison implementation to use when comparing keys.
        /// </param>
        public RWLockDictionary(LockRecursionPolicy recursionPolicy, IEqualityComparer<TKey> equalityComparer)
            : base(recursionPolicy)
        {
            locks = new ConcurrentDictionary<TKey, Lock>(equalityComparer);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="key">
        /// The key of the locker.
        /// </param>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockRead(TKey key, Action block)
        {
            LockRead(() =>
            {
                Lock pathLock = locks.GetOrAdd(key, _ => new Lock(this, key));
                Interlocked.Increment(ref pathLock.Lockers);
                pathLock.RWLock.LockRead(block);
                if (pathLock.Lockers <= 1)
                {
                    locks.TryRemove(key, out _);
                }
                else
                {
                    Interlocked.Decrement(ref pathLock.Lockers);
                }
            });
        }

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> function.
        /// </summary>
        /// <typeparam name="TReturn">
        /// The object type returned by the <paramref name="block"/> function.
        /// </typeparam>
        /// <param name="key">
        /// The key of the locker.
        /// </param>
        /// <param name="block">
        /// The function to be executed inside the lock block.
        /// </param>
        /// <returns>
        /// The object returned by the <paramref name="block"/> function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public TReturn LockRead<TReturn>(TKey key, Func<TReturn> block)
        {
            return LockRead(() =>
            {
                Lock pathLock = locks.GetOrAdd(key, _ => new Lock(this, key));
                Interlocked.Increment(ref pathLock.Lockers);
                TReturn ret = pathLock.RWLock.LockRead(block);
                Interlocked.Decrement(ref pathLock.Lockers);
                if (pathLock.Lockers <= 1)
                {
                    locks.TryRemove(key, out _);
                }
                else
                {
                    Interlocked.Decrement(ref pathLock.Lockers);
                }
                return ret;
            });
        }

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> action with the option upgrade to write mode.
        /// </summary>
        /// <param name="key">
        /// The key of the locker.
        /// </param>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockReadUpgradable(TKey key, Action block)
        {
            LockRead(() =>
            {
                Lock pathLock = locks.GetOrAdd(key, _ => new Lock(this, key));
                Interlocked.Increment(ref pathLock.Lockers);
                pathLock.RWLock.LockReadUpgradable(block);
                if (pathLock.Lockers <= 1)
                {
                    locks.TryRemove(key, out _);
                }
                else
                {
                    Interlocked.Decrement(ref pathLock.Lockers);
                }
            });
        }

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> function with the option upgrade to write mode.
        /// </summary>
        /// <typeparam name="TReturn">
        /// The object type returned by the <paramref name="block"/> function.
        /// </typeparam>
        /// <param name="key">
        /// The key of the locker.
        /// </param>
        /// <param name="block">
        /// The function to be executed inside the lock block.
        /// </param>
        /// <returns>
        /// The object returned by the <paramref name="block"/> function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public TReturn LockReadUpgradable<TReturn>(TKey key, Func<TReturn> block)
        {
            return LockRead(() =>
            {
                Lock pathLock = locks.GetOrAdd(key, _ => new Lock(this, key));
                Interlocked.Increment(ref pathLock.Lockers);
                TReturn ret = pathLock.RWLock.LockReadUpgradable(block);
                Interlocked.Decrement(ref pathLock.Lockers);
                if (pathLock.Lockers <= 1)
                {
                    locks.TryRemove(key, out _);
                }
                else
                {
                    Interlocked.Decrement(ref pathLock.Lockers);
                }
                return ret;
            });
        }

        /// <summary>
        /// Locks write operations while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="key">
        /// The key of the locker.
        /// </param>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockWrite(TKey key, Action block)
        {
            LockRead(() =>
            {
                Lock pathLock = locks.GetOrAdd(key, _ => new Lock(this, key));
                Interlocked.Increment(ref pathLock.Lockers);
                pathLock.RWLock.LockWrite(block);
                Interlocked.Decrement(ref pathLock.Lockers);
                if (pathLock.Lockers <= 1)
                {
                    locks.TryRemove(key, out _);
                }
                else
                {
                    Interlocked.Decrement(ref pathLock.Lockers);
                }
            });
        }

        /// <summary>
        /// Locks write operations while executing the <paramref name="block"/> function.
        /// </summary>
        /// <typeparam name="TReturn">
        /// The object type returned by the <paramref name="block"/> function.
        /// </typeparam>
        /// <param name="key">
        /// The key of the locker.
        /// </param>
        /// <param name="block">
        /// The function to be executed inside the lock block.
        /// </param>
        /// <returns>
        /// The object returned by the <paramref name="block"/> function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public TReturn LockWrite<TReturn>(TKey key, Func<TReturn> block)
        {
            return LockRead(() =>
            {
                Lock pathLock = locks.GetOrAdd(key, _ => new Lock(this, key));
                Interlocked.Increment(ref pathLock.Lockers);
                TReturn ret = pathLock.RWLock.LockWrite(block);
                Interlocked.Decrement(ref pathLock.Lockers);
                if (pathLock.Lockers <= 1)
                {
                    locks.TryRemove(key, out _);
                }
                else
                {
                    Interlocked.Decrement(ref pathLock.Lockers);
                }
                return ret;
            });
        }

        #endregion

        #region Helper Classes

        private class Lock
        {
            public int Lockers = 0;

            public readonly RWLockDictionary<TKey> Dictionary;
            public readonly TKey Path;
            public readonly RWLock RWLock;

            public Lock(RWLockDictionary<TKey> dictionary, TKey path)
            {
                Dictionary = dictionary;
                Path = path;
                RWLock = new RWLock(dictionary.ReaderWriterLockSlim.RecursionPolicy);
            }
        }

        #endregion
    }
}
