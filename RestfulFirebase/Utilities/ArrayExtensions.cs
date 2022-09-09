using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Utilities;

/// <summary>
/// Provides extension methods for <see cref="Array"/>.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Removes an element of the <paramref name="array"/> at the specified <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the element of the <paramref name="array"/>.
    /// </typeparam>
    /// <param name="array">
    /// The array to remove an element to.
    /// </param>
    /// <param name="index">
    /// The index of the element to remove.
    /// </param>
    /// <returns>
    /// The newly created <see cref="Array"/> with the removed element at the specified <paramref name="index"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is below zero or is greater than or equal to the length of <paramref name="array"/>.
    /// </exception>
    public static T[] RemoveAt<T>(this T[] array, int index)
    {
        if (array == null)
        {
            ArgumentNullException.ThrowIfNull(array);
        }
        if (index < 0 || index >= array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        T[] modified = new T[array.Length - 1];
        if (index > 0)
        {
            Array.Copy(array, 0, modified, 0, index);
        }
        if (index < array.Length - 1)
        {
            Array.Copy(array, index + 1, modified, index, array.Length - index - 1);
        }
        return modified;
    }
}
