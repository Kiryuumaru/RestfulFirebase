using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace RestfulFirebase.FirestoreDatabase.Writes;

/// <summary>
/// The parameter for write commits.
/// </summary>
public abstract partial class Write
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

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

    internal readonly List<(object model, CollectionReference collectionReference, string? documentName)> WritableCreateDocuments;
    internal readonly List<Document> WritablePatchDocuments;
    internal readonly List<DocumentReference> WritableDeleteDocuments;
    internal readonly List<DocumentTransform> WritableTransformDocuments;

    internal Write(FirebaseApp app)
    {
        App = app;

        WritableCreateDocuments = new();
        WritablePatchDocuments = new();
        WritableDeleteDocuments = new();
        WritableTransformDocuments = new();
        CreateDocuments = WritableCreateDocuments.AsReadOnly();
        PatchDocuments = WritablePatchDocuments.AsReadOnly();
        DeleteDocuments = WritableDeleteDocuments.AsReadOnly();
        TransformDocuments = WritableTransformDocuments.AsReadOnly();
    }

    internal Write(Write write)
    {
        App = write.App;

        WritableCreateDocuments = write.WritableCreateDocuments;
        WritablePatchDocuments = write.WritablePatchDocuments;
        WritableDeleteDocuments = write.WritableDeleteDocuments;
        WritableTransformDocuments = write.WritableTransformDocuments;
        CreateDocuments = write.CreateDocuments;
        PatchDocuments = write.PatchDocuments;
        DeleteDocuments = write.DeleteDocuments;
        TransformDocuments = write.TransformDocuments;
    }
}

/// <inheritdoc/>
public abstract partial class FluentWriteRoot<TWrite> : Write
    where TWrite : FluentWriteRoot<TWrite>
{
    internal FluentWriteRoot(FirebaseApp app)
        : base(app)
    {

    }

    internal FluentWriteRoot(Write write)
        : base(write)
    {

    }
}

/// <inheritdoc/>
public abstract partial class FluentWriteWithDocumentTransform<TWrite> : FluentWriteRoot<TWrite>
    where TWrite : FluentWriteWithDocumentTransform<TWrite>
{
    internal FluentWriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal FluentWriteWithDocumentTransform(Write write)
        : base(write)
    {

    }
}

/// <inheritdoc/>
public abstract partial class FluentWriteWithDocumentTransform<TWrite, TModel> : FluentWriteWithDocumentTransform<TWrite>
    where TWrite : FluentWriteWithDocumentTransform<TWrite, TModel>
    where TModel : class
{
    internal FluentWriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal FluentWriteWithDocumentTransform(Write write)
        : base(write)
    {

    }
}

/// <inheritdoc/>
public abstract partial class FluentWriteWithCacheAndDocumentTransform<TWrite> : FluentWriteWithDocumentTransform<TWrite>
    where TWrite : FluentWriteWithCacheAndDocumentTransform<TWrite>
{
    /// <summary>
    /// Gets the list of cache <see cref="Document"/>.
    /// </summary>
    public IReadOnlyList<Document> CacheDocuments { get; }

    internal readonly List<Document> WritableCacheDocuments;

    internal FluentWriteWithCacheAndDocumentTransform(FirebaseApp app)
        : base(app)
    {
        WritableCacheDocuments = new();
        CacheDocuments = WritableCacheDocuments.AsReadOnly();
    }

    internal FluentWriteWithCacheAndDocumentTransform(Write write)
        : base(write)
    {
        if (write is FluentWriteWithCacheAndDocumentTransform<TWrite> writeWithCache)
        {
            WritableCacheDocuments = writeWithCache.WritableCacheDocuments;
            CacheDocuments = writeWithCache.CacheDocuments;
        }
        else
        {
            WritableCacheDocuments = new();
            CacheDocuments = WritableCacheDocuments.AsReadOnly();
        }
    }
}

/// <inheritdoc/>
public abstract partial class FluentWriteWithCacheAndDocumentTransform<TWrite, TModel> : FluentWriteWithDocumentTransform<TWrite, TModel>
    where TWrite : FluentWriteWithCacheAndDocumentTransform<TWrite, TModel>
    where TModel : class
{
    /// <summary>
    /// Gets the list of cache <see cref="Document"/>.
    /// </summary>
    public IReadOnlyList<Document> CacheDocuments { get; }

    internal readonly List<Document> WritableCacheDocuments;

    internal FluentWriteWithCacheAndDocumentTransform(FirebaseApp app)
        : base(app)
    {
        WritableCacheDocuments = new();
        CacheDocuments = WritableCacheDocuments.AsReadOnly();
    }

    internal FluentWriteWithCacheAndDocumentTransform(Write write)
        : base(write)
    {
        if (write is FluentWriteWithCacheAndDocumentTransform<TWrite, TModel> writeWithCache)
        {
            WritableCacheDocuments = writeWithCache.WritableCacheDocuments;
            CacheDocuments = writeWithCache.CacheDocuments;
        }
        else
        {
            WritableCacheDocuments = new();
            CacheDocuments = WritableCacheDocuments.AsReadOnly();
        }
    }
}

#region Instantiable

/// <inheritdoc/>
public partial class WriteRoot : FluentWriteRoot<WriteRoot>
{
    internal WriteRoot(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteRoot(Write write)
        : base(write)
    {

    }
}

/// <inheritdoc/>
public partial class WriteWithDocumentTransform : FluentWriteWithDocumentTransform<WriteWithDocumentTransform>
{
    internal WriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteWithDocumentTransform(Write write)
        : base(write)
    {

    }
}

/// <inheritdoc/>
public partial class WriteWithDocumentTransform<TModel> : FluentWriteWithDocumentTransform<WriteWithDocumentTransform<TModel>, TModel>
    where TModel : class
{
    internal WriteWithDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteWithDocumentTransform(Write write)
        : base(write)
    {

    }
}

/// <inheritdoc/>
public partial class WriteWithCacheAndDocumentTransform : FluentWriteWithCacheAndDocumentTransform<WriteWithCacheAndDocumentTransform>
{
    internal WriteWithCacheAndDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteWithCacheAndDocumentTransform(Write write)
        : base(write)
    {

    }
}

/// <inheritdoc/>
public partial class WriteWithCacheAndDocumentTransform<TModel> : FluentWriteWithCacheAndDocumentTransform<WriteWithCacheAndDocumentTransform<TModel>, TModel>
    where TModel : class
{
    internal WriteWithCacheAndDocumentTransform(FirebaseApp app)
        : base(app)
    {

    }

    internal WriteWithCacheAndDocumentTransform(Write write)
        : base(write)
    {

    }
}

#endregion