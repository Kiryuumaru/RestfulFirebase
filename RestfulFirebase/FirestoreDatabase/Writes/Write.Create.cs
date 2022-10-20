using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading;
using RestfulFirebase.Common.Http;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Transactions;

namespace RestfulFirebase.FirestoreDatabase.Writes;

public abstract partial class Write
{
}

public partial class FluentWriteRoot<TWrite>
{
    /// <summary>
    /// Adds a documents to perform a create operation. This operation is not atomic and does not include with the <see cref="Write.PatchDocuments"/>, <see cref="Write.DeleteDocuments"/> and <see cref="Write.TransformDocuments"/> operations.
    /// </summary>
    /// <param name="model">
    /// The model to create the document.
    /// </param>
    /// <param name="collectionReference">
    /// The <see cref="CollectionReference"/> to create document.
    /// </param>
    /// <param name="documentId">
    /// The client-assigned document ID to use for this document. Optional. If not specified, an ID will be assigned by the service.
    /// </param>
    /// <returns>
    /// The write with new added documents to patch.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="model"/> or
    /// <paramref name="collectionReference"/> is a null reference.
    /// </exception>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public TWrite Create<TModel>(TModel model, CollectionReference collectionReference, string? documentId = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(collectionReference);

        WritableCreateDocuments.Add((model, collectionReference, documentId));

        return (TWrite)this;
    }
}
