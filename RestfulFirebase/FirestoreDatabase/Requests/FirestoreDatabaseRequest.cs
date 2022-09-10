using RestfulFirebase.Authentication;
using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.CloudFirestore.Requests;

/// <summary>
/// The base implementation for all firebase cloud firestore request.
/// </summary>
public class FirestoreDatabaseRequest : AuthenticatedCommonRequest
{
    internal Query.Query? Query { get; set; }
}
