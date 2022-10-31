using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.FirestoreDatabase.Writes;

/// <summary>
/// The parameter for write commits.
/// </summary>
public abstract partial class Write : ICloneable<Write>
{
    /// <summary>
    /// Gets the list of <see cref="Document"/> to perform create.
    /// </summary>
    public IReadOnlyList<(object model, CollectionReference collectionReference, string? documentName)> CreateDocuments { get; }

    /// <summary>
    /// Gets the list of <see cref="Document"/> to perform patch.
    /// </summary>
    public IReadOnlyList<Document> PatchDocuments { get; }

    /// <summary>
    /// Gets the list of <see cref="Document"/> to perform delete.
    /// </summary>
    public IReadOnlyList<DocumentReference> DeleteDocuments { get; }

    /// <summary>
    /// Gets the list of <see cref="DocumentTransform"/> to perform transform.
    /// </summary>
    public IReadOnlyList<DocumentTransform> TransformDocuments { get; }

    /// <summary>
    /// Gets the list of cache <see cref="Document"/>.
    /// </summary>
    public IReadOnlyList<Document> CacheDocuments { get; }

    /// <summary>
    /// The <see cref="Transaction"/> to optionally perform an atomic operation.
    /// </summary>
    public Transaction? TransactionUsed { get; internal set; }

    /// <summary>
    /// Gets the authorization used for the operation.
    /// </summary>
    public IAuthorization? AuthorizationUsed { get; protected set; }

    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    internal readonly List<(object model, CollectionReference collectionReference, string? documentName)> WritableCreateDocuments;
    internal readonly List<Document> WritablePatchDocuments;
    internal readonly List<DocumentReference> WritableDeleteDocuments;
    internal readonly List<DocumentTransform> WritableTransformDocuments;
    internal readonly List<Document> WritableCacheDocuments;

    internal Write(FirebaseApp app)
    {
        App = app;

        WritableCreateDocuments = new();
        WritablePatchDocuments = new();
        WritableDeleteDocuments = new();
        WritableTransformDocuments = new();
        WritableCacheDocuments = new();

        CreateDocuments = WritableCreateDocuments.AsReadOnly();
        PatchDocuments = WritablePatchDocuments.AsReadOnly();
        DeleteDocuments = WritableDeleteDocuments.AsReadOnly();
        TransformDocuments = WritableTransformDocuments.AsReadOnly();
        CacheDocuments = WritableCacheDocuments.AsReadOnly();
    }

    internal Write(Write write, bool isClone)
    {
        App = write.App;
        TransactionUsed = write.TransactionUsed;
        AuthorizationUsed = write.AuthorizationUsed;

        if (isClone)
        {
            WritableCreateDocuments = new(write.WritableCreateDocuments);
            WritablePatchDocuments = new(write.WritablePatchDocuments);
            WritableDeleteDocuments = new(write.WritableDeleteDocuments);
            WritableTransformDocuments = new(write.WritableTransformDocuments);
            WritableCacheDocuments = new(write.WritableCacheDocuments);

            CreateDocuments = WritableCreateDocuments.AsReadOnly();
            PatchDocuments = WritablePatchDocuments.AsReadOnly();
            DeleteDocuments = WritableDeleteDocuments.AsReadOnly();
            TransformDocuments = WritableTransformDocuments.AsReadOnly();
            CacheDocuments = WritableCacheDocuments.AsReadOnly();
        }
        else
        {
            WritableCreateDocuments = write.WritableCreateDocuments;
            WritablePatchDocuments = write.WritablePatchDocuments;
            WritableDeleteDocuments = write.WritableDeleteDocuments;
            WritableTransformDocuments = write.WritableTransformDocuments;
            WritableCacheDocuments = write.WritableCacheDocuments;

            CreateDocuments = write.CreateDocuments;
            PatchDocuments = write.PatchDocuments;
            DeleteDocuments = write.DeleteDocuments;
            TransformDocuments = write.TransformDocuments;
            CacheDocuments = write.CacheDocuments;
        }
    }

    /// <inheritdoc/>
    public Write Clone() => (Write)CoreClone();

    /// <inheritdoc/>
    object ICloneable.Clone() => CoreClone();

    /// <inheritdoc/>
    protected abstract object CoreClone();
}

/// <inheritdoc/>
public abstract partial class FluentWriteRoot<TWrite> : Write, ICloneable<FluentWriteRoot<TWrite>>
    where TWrite : FluentWriteRoot<TWrite>
{
    internal FluentWriteRoot(FirebaseApp app)
        : base(app)
    {

    }

    internal FluentWriteRoot(Write write, bool isClone)
        : base(write, isClone)
    {

    }

    /// <inheritdoc/>
    public new FluentWriteRoot<TWrite> Clone() => (FluentWriteRoot<TWrite>)CoreClone();
}

/// <inheritdoc/>
public abstract partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>, ICloneable<FluentWriteWithDocumentTransform<TWrite>>
    where TWrite : FluentWriteWithDocumentTransform<TWrite>
{
    internal FluentWriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal FluentWriteWithDocumentTransform(Write write, bool isClone)
        : base(write, isClone)
    {

    }

    /// <inheritdoc/>
    public new FluentWriteWithDocumentTransform<TWrite> Clone() => (FluentWriteWithDocumentTransform<TWrite>)CoreClone();
}

/// <inheritdoc/>
public abstract partial class FluentWriteWithDocumentTransform<TWrite, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentWriteWithDocumentTransform<TWrite>, ICloneable<FluentWriteWithDocumentTransform<TWrite, TModel>>
    where TWrite : FluentWriteWithDocumentTransform<TWrite, TModel>
    where TModel : class
{
    internal FluentWriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal FluentWriteWithDocumentTransform(Write write, bool isClone)
        : base(write, isClone)
    {

    }

    /// <inheritdoc/>
    public new FluentWriteWithDocumentTransform<TWrite, TModel> Clone() => (FluentWriteWithDocumentTransform<TWrite, TModel>)CoreClone();
}

#region Instantiable

/// <inheritdoc/>
public class WriteRoot : FluentWriteRoot<WriteRoot>, ICloneable<WriteRoot>
{
    internal WriteRoot(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteRoot(Write write, bool isClone)
        : base(write, isClone)
    {

    }

    /// <inheritdoc/>
    public new WriteRoot Clone() => (WriteRoot)CoreClone();

    /// <inheritdoc/>
    protected override object CoreClone() => new WriteRoot(this, true);
}

/// <inheritdoc/>
public class WriteWithDocumentTransform : FluentWriteWithDocumentTransform<WriteWithDocumentTransform>, ICloneable<WriteWithDocumentTransform>
{
    internal WriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteWithDocumentTransform(Write write, bool isClone)
        : base(write, isClone)
    {

    }

    /// <inheritdoc/>
    public new WriteWithDocumentTransform Clone() => (WriteWithDocumentTransform)CoreClone();

    /// <inheritdoc/>
    protected override object CoreClone() => new WriteWithDocumentTransform(this, true);
}

/// <inheritdoc/>
public class WriteWithDocumentTransform<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TModel> : FluentWriteWithDocumentTransform<WriteWithDocumentTransform<TModel>, TModel>, ICloneable<WriteWithDocumentTransform<TModel>>
    where TModel : class
{
    internal WriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteWithDocumentTransform(Write write, bool isClone)
        : base(write, isClone)
    {

    }

    /// <inheritdoc/>
    public new WriteWithDocumentTransform<TModel> Clone() => (WriteWithDocumentTransform<TModel>)CoreClone();

    /// <inheritdoc/>
    protected override object CoreClone() => new WriteWithDocumentTransform<TModel>(this, true);
}

#endregion