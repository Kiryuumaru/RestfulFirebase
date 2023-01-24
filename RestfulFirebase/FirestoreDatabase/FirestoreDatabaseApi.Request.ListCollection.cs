using System.Text.Json;
using RestfulFirebase.FirestoreDatabase.References;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Request to list the <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="pageSize">
    /// The requested page size of the pager <see cref="ListCollectionResult.GetAsyncEnumerator(CancellationToken)"/>.
    /// </param>
    /// <param name="documentReference">
    /// The requested <see cref="DocumentReference"/> of the document node.
    /// </param>
    /// <param name="authorization">
    /// The authorization used for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/> with the result <see cref="ListCollectionResult"/>.
    /// </returns>
    public async Task<HttpResponse<ListCollectionResult>> ListCollection(int? pageSize = null, DocumentReference? documentReference = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
    {
        JsonSerializerOptions jsonSerializerOptions = ConfigureJsonSerializerOption();

        return await ExecuteListCollectionNextPage(new(), null, pageSize, documentReference, authorization, jsonSerializerOptions, cancellationToken);
    }
}
