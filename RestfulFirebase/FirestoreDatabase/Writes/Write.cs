using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

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

    internal readonly List<Document> WritablePatchDocuments;
    internal readonly List<DocumentReference> WritableDeleteDocuments;
    internal readonly List<DocumentTransform> WritableTransformDocuments;

    internal Write(FirebaseApp app)
    {
        App = app;

        WritablePatchDocuments = new();
        WritableDeleteDocuments = new();
        WritableTransformDocuments = new();
        PatchDocuments = WritablePatchDocuments.AsReadOnly();
        DeleteDocuments = WritableDeleteDocuments.AsReadOnly();
        TransformDocuments = WritableTransformDocuments.AsReadOnly();
    }

    internal Write(Write write)
    {
        App = write.App;

        WritablePatchDocuments = write.WritablePatchDocuments;
        WritableDeleteDocuments = write.WritableDeleteDocuments;
        WritableTransformDocuments = write.WritableTransformDocuments;
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

#endregion