using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when the argument string is null or empty.
/// </summary>
public class StringNullOrEmptyException : ArgumentException
{
    private StringNullOrEmptyException(string message)
        : base(message)
    {

    }

    internal static StringNullOrEmptyException FromSingleArgument(string argumentName)
    {
        return new StringNullOrEmptyException("Argument string '" + argumentName + "' is null or empty.");
    }

    internal static StringNullOrEmptyException FromEnumerableArgument(string argumentName)
    {
        return new StringNullOrEmptyException("Argument enumerable string '" + argumentName + "' has null or empty element.");
    }
}
