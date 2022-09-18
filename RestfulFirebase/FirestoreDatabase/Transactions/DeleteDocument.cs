﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.Common.Transactions;
using System.Threading.Tasks;
using System.Net.Http;
using RestfulFirebase.Common;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Xml.Linq;

namespace RestfulFirebase.FirestoreDatabase.Transactions;

/// <summary>
/// Request to delete the <see cref="Document{T}"/> of the specified request query.
/// </summary>
public class DeleteDocumentRequest : FirestoreDatabaseRequest<TransactionResponse<DeleteDocumentRequest>>
{
    /// <summary>
    /// Gets or sets the requested <see cref="Queries.DocumentReference"/> of the document node.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <inheritdoc cref="DeleteDocumentRequest"/>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="TransactionResponse"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <see cref="TransactionRequest.Config"/> or
    /// <see cref="DocumentReference"/> is a null reference.
    /// </exception>
    internal override async Task<TransactionResponse<DeleteDocumentRequest>> Execute()
    {
        ArgumentNullException.ThrowIfNull(Config);
        ArgumentNullException.ThrowIfNull(DocumentReference);

        try
        {
            await Execute(HttpMethod.Delete, DocumentReference.BuildUrl(Config.ProjectId));

            return new(this, null);
        }
        catch (Exception ex)
        {
            return new(this, ex);
        }
    }
}
