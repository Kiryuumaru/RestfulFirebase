﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RestfulFirebase.CloudFirestore.Query;
using RestfulFirebase.FirestoreDatabase;

namespace RestfulFirebase.CloudFirestore.Requests;

/// <summary>
/// Request to patch the <see cref="Document{T}"/> of the specified request query.
/// </summary>
/// <typeparam name="T">
/// The type of the model to populate the document fields.
/// </typeparam>
public class PatchDocumentRequest<T> : FirestoreDatabaseRequest
    where T : class
{
    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used to serialize and deserialize documents.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the existing <typeparamref name="T"/> model to populate the document fields.
    /// </summary>
    public T? Model { get; set; }

    /// <summary>
    /// Gets or sets the existing <see cref="Document{T}"/> to populate the document fields.
    /// </summary>
    public Document<T>? Document { get; set; }

    /// <summary>
    /// Gets or sets the requested <see cref="DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? Reference
    {
        get => Query as DocumentReference;
        set => Query = value;
    }
}
