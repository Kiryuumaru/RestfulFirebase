using System;

namespace RestfulFirebase.Exceptions;

/// <summary>
/// Occurs when there`s an error in storage.
/// </summary>
public abstract class StorageException : Exception
{
    private protected StorageException()
    {

    }

    private protected StorageException(Exception innerException)
        : base("A storage error occured.", innerException)
    {

    }

    private protected StorageException(string message)
        : base(message)
    {

    }

    private protected StorageException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
