//using System.Diagnostics.CodeAnalysis;
//using System.Threading.Tasks;
//using System.Threading;
//using RestfulFirebase.FirestoreDatabase.Transactions;
//using RestfulFirebase.Common.Http;
//using RestfulFirebase.Common.Abstractions;
//using RestfulFirebase.FirestoreDatabase.Writes;

//namespace RestfulFirebase.RealtimeDatabase.Models;

//public partial class Snapshot
//{
//    /// <summary>
//    /// Request to perform a get operation to document.
//    /// </summary>
//    /// <param name="transaction">
//    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
//    /// </param>
//    /// <param name="authorization">
//    /// The authorization used for the operation.
//    /// </param>
//    /// <param name="cancellationToken">
//    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
//    /// </param>
//    /// <returns>
//    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
//    /// </returns>
//    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
//    public async Task<HttpResponse> Get(Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
//    {
//        HttpResponse response = new();

//        var getResponse = await Reference.App.FirestoreDatabase.Fetch()
//            .Document(this)
//            .Transaction(transaction)
//            .Authorization(authorization)
//            .Run(cancellationToken);
//        response.Append(getResponse);
//        if (getResponse.IsError)
//        {
//            return response;
//        }

//        return response;
//    }

//    /// <summary>
//    /// Request to perform a patch and get operation to document.
//    /// </summary>
//    /// <param name="transaction">
//    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
//    /// </param>
//    /// <param name="authorization">
//    /// The authorization used for the operation.
//    /// </param>
//    /// <param name="cancellationToken">
//    /// The <see cref="CancellationToken"/> that propagates notification if the operations should be canceled.
//    /// </param>
//    /// <returns>
//    /// The <see cref="Task"/> proxy that represents the <see cref="HttpResponse"/>.
//    /// </returns>
//    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
//    public async Task<HttpResponse> PatchAndGet(Transaction? transaction = default, IAuthorization? authorization = default, CancellationToken cancellationToken = default)
//    {
//        HttpResponse response = new();

//        var writeResponse = await Reference.App.FirestoreDatabase.Write()
//            .Patch(this)
//            .Transaction(transaction)
//            .Authorization(authorization)
//            .RunAndGet(cancellationToken);
//        response.Append(writeResponse);
//        if (writeResponse.IsError)
//        {
//            return response;
//        }

//        return response;
//    }

//    /// <summary>
//    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
//    /// </summary>
//    /// <returns>
//    /// The write with new added <see cref="DocumentTransform"/> to transform.
//    /// </returns>
//    public WriteWithDocumentTransform Transform()
//    {
//        return new WriteWithDocumentTransform(Reference.Transform(), true)
//            .Cache(this);
//    }

//    /// <summary>
//    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
//    /// </summary>
//    /// <typeparam name="TModel">
//    /// The type of the model of the document to transform.
//    /// </typeparam>
//    /// <returns>
//    /// The write with new added <see cref="DocumentTransform"/> to transform.
//    /// </returns>
//    public WriteWithDocumentTransform<TModel> Transform<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>()
//        where TModel : class
//    {
//        return new WriteWithDocumentTransform<TModel>(Reference.Transform<TModel>(), true)
//            .Cache(this);
//    }
//}

//public partial class Document<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel>
//{
//    /// <summary>
//    /// Adds new <see cref="DocumentTransform"/> to perform a transform operation.
//    /// </summary>
//    /// <returns>
//    /// The write with new added <see cref="DocumentTransform"/> to transform.
//    /// </returns>
//    public new WriteWithDocumentTransform<TModel> Transform()
//    {
//        return new WriteWithDocumentTransform<TModel>(Reference.Transform<TModel>(), true)
//            .Cache(this);
//    }
//}
