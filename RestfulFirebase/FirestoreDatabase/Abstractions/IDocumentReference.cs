using RestfulFirebase.FirestoreDatabase.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestfulFirebase.FirestoreDatabase.Abstractions;

/// <summary>
/// A document reference that represents the document node location.
/// </summary>
public interface IDocumentReference
{
    /// <summary>
    /// Gets the document reference of the document node.
    /// </summary>
    DocumentReference Reference { get; }
}