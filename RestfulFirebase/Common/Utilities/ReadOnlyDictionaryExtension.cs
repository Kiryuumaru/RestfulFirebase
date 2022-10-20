﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace RestfulFirebase.Common.Utilities;

internal static class ReadOnlyDictionaryExtension
{
    public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        return new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }

    internal class ReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        public TValue this[TKey key] => dictionary[key];

        public IEnumerable<TKey> Keys => dictionary.Keys;

        public IEnumerable<TValue> Values => dictionary.Values;

        public int Count => dictionary.Count;

        private readonly IDictionary<TKey, TValue> dictionary;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = dictionary;
        }

        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => dictionary.TryGetValue(key, out value);
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

        IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();
    }
}
