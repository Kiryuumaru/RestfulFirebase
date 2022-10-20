﻿using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.Common.Http;
using RestfulFirebase.Common.Abstractions;

namespace RestfulFirebase.FirestoreDatabase.Models;

public partial class Document
{
    /// <summary>
    /// Request to perform a get operation to document.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse> Get(Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var writeResponse = await Reference.GetDocument(new Document[] { this }, transaction, authorization, cancellationToken);
        response.Append(writeResponse);
        if (writeResponse.IsError)
        {
            return response;
        }

        return response;
    }

    /// <summary>
    /// Request to perform a patch and get operation to document.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
    /// </returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public async Task<HttpResponse> PatchAndGet(Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        HttpResponse response = new();

        var writeResponse = await Reference.PatchAndGetDocument(GetModel(), new Document[] { this }, transaction, authorization, cancellationToken);
        response.Append(writeResponse);
        if (writeResponse.IsError)
        {
            return response;
        }

        return response;
    }
}

public partial class Document<T>
{

}
