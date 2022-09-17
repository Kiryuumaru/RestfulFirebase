﻿using RestfulFirebase.Authentication;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The base request for all firebase request.
/// </summary>
public interface ITransactionRequest
{
    /// <summary>
    /// Gets or sets the config of the request.
    /// </summary>
    public FirebaseConfig? Config { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Net.Http.HttpClient"/> used for the request.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="System.Threading.CancellationToken"/> of the request.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
}