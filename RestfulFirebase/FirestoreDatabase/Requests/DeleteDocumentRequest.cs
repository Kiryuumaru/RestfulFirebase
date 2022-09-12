using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.CloudFirestore.Query;
using RestfulFirebase.FirestoreDatabase;

namespace RestfulFirebase.CloudFirestore.Requests;

/// <summary>
/// Request to delete the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class DeleteDocumentRequest : FirestoreDatabaseRequest
{
    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? Reference
    {
        get => Query as DocumentReference;
        set => Query = value;
    }
}
