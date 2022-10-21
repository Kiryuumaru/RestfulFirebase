using RestfulFirebase.FirestoreDatabase.References;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Linq;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public abstract partial class Write
{
}

public partial class FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds a documents to perform a delete operation.
    /// </summary>
    /// <param name="documents">
    /// The documents to delete.
    /// </param>
    /// <returns>
    /// The write with new added documents to delete.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Delete(params Document[] documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        WritableDeleteDocuments.AddRange(documents.Select(i => i.Reference));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds a documents to perform a delete operation.
    /// </summary>
    /// <param name="documents">
    /// The documents to delete.
    /// </param>
    /// <returns>
    /// The write with new added documents to delete.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Delete(IEnumerable<Document> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        WritableDeleteDocuments.AddRange(documents.Select(i => i.Reference));

        return (TWrite)this;
    }

    /// <summary>
    /// Adds a documents to perform a delete operation.
    /// </summary>
    /// <param name="documentReferences">
    /// The documents to delete.
    /// </param>
    /// <returns>
    /// The write with new added documents to delete.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentReferences"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Delete(params DocumentReference[] documentReferences)
    {
        ArgumentNullException.ThrowIfNull(documentReferences);

        WritableDeleteDocuments.AddRange(documentReferences);

        return (TWrite)this;
    }

    /// <summary>
    /// Adds a documents to perform a delete operation.
    /// </summary>
    /// <param name="documentReferences">
    /// The documents to delete.
    /// </param>
    /// <returns>
    /// The write with new added documents to delete.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documentReferences"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Delete(IEnumerable<DocumentReference> documentReferences)
    {
        ArgumentNullException.ThrowIfNull(documentReferences);

        WritableDeleteDocuments.AddRange(documentReferences);

        return (TWrite)this;
    }
}
