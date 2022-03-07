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

    /// <summary>
    /// Creates an instance of <see cref="StringNullOrEmptyException"/> with provided <paramref name="argumentName"/>.
    /// </summary>
    /// <param name="argumentName">
    /// The name of the <see cref="string"/> typed argument that is null or empty.
    /// </param>
    public static StringNullOrEmptyException FromSingleArgument(string argumentName)
    {
        return new StringNullOrEmptyException("Argument string '" + argumentName + "' is null or empty.");
    }

    /// <summary>
    /// Creates an instance of <see cref="StringNullOrEmptyException"/> with provided <paramref name="argumentName"/>.
    /// </summary>
    /// <param name="argumentName">
    /// The name of the <see cref="T:string[]"/>typed  argument that is null or empty.
    /// </param>
    public static StringNullOrEmptyException FromEnumerableArgument(string argumentName)
    {
        return new StringNullOrEmptyException("Argument enumerable string '" + argumentName + "' has null or empty element.");
    }
}
