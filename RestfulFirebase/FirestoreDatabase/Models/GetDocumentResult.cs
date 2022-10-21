using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// The result for get document operations.
/// </summary>
public class GetDocumentResult
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public DocumentTimestamp? Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public DocumentReferenceTimestamp? Missing { get; }

    internal GetDocumentResult(DocumentTimestamp? found, DocumentReferenceTimestamp? missing)
    {
        Found = found;
        Missing = missing;
    }
}

/// <summary>
/// The result for get document operations.
/// </summary>
/// <typeparam name="TModel">
/// The type of the model of the document.
/// </typeparam>
public class GetDocumentResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>
    where TModel : class
{
    /// <summary>
    /// Gets the found document.
    /// </summary>
    public DocumentTimestamp<TModel>? Found { get; }

    /// <summary>
    /// Gets the missing document.
    /// </summary>
    public DocumentReferenceTimestamp? Missing { get; }

    internal GetDocumentResult(DocumentTimestamp<TModel>? found, DocumentReferenceTimestamp? missing)
    {
        Found = found;
        Missing = missing;
    }
}
