using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Common.Exceptions;

/// <summary>
/// Occurs when the argument string is null or empty.
/// </summary>
public class StringNullOrEmptyException : ArgumentException
{
    private StringNullOrEmptyException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Throws an <see cref="StringNullOrEmptyException"/> if <paramref name="argument"/> is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="argument">The reference type argument to validate as non-<see langword="null"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null || string.IsNullOrEmpty(argument))
        {
            throw new StringNullOrEmptyException("Argument string '" + paramName + "' is null or empty.");
        }
    }

    /// <summary>
    /// Throws an <see cref="StringNullOrEmptyException"/> if <paramref name="argument"/> is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="argument">
    /// The reference type argument to validate as non-<see langword="null"/> or non-empty.
    /// </param>
    /// <param name="paramName">
    /// The name of the parameter with which <paramref name="argument"/> corresponds.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty([NotNull] IEnumerable<string?>? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null || argument.Any(i => string.IsNullOrEmpty(i)))
        {
            throw new StringNullOrEmptyException("Argument enumerable string '" + paramName + "' has null or empty element.");
        }
    }
}
