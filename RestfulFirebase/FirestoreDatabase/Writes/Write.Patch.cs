using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public abstract partial class Write
{
}

public partial class FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds a documents to perform a patch operation.
    /// </summary>
    /// <param name="documents">
    /// The documents to patch.
    /// </param>
    /// <returns>
    /// The write with new added documents to patch.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Patch(params Document[] documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        WritablePatchDocuments.AddRange(documents);

        return (TWrite)this;
    }

    /// <summary>
    /// Adds a documents to perform a patch operation.
    /// </summary>
    /// <param name="documents">
    /// The documents to patch.
    /// </param>
    /// <returns>
    /// The write with new added documents to patch.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="documents"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Patch(IEnumerable<Document> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        WritablePatchDocuments.AddRange(documents);

        return (TWrite)this;
    }
}
