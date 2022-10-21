using System.Collections.Generic;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The result for get documents operations.
/// </summary>
public class GetDocumentsResult
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReferenceTimestamp> Missing { get; }

    internal GetDocumentsResult(IReadOnlyList<DocumentTimestamp> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}

/// <summary>
/// The result for get documents operations.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public class GetDocumentsResult<T>
    where T : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public IReadOnlyList<DocumentTimestamp<T>> Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public IReadOnlyList<DocumentReferenceTimestamp> Missing { get; }

    internal GetDocumentsResult(IReadOnlyList<DocumentTimestamp<T>> found, IReadOnlyList<DocumentReferenceTimestamp> missing)
    {
        Found = found;
        Missing = missing;
    }
}
