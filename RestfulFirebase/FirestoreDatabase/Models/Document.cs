using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using RestfulFirebase.FirestoreDatabase.Queries;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json.Serialization;

namespace RestfulFirebase.FirestoreDatabase.Models;
/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
[ObservableObject]
public partial class Document
{
    #region Properties

    /// <summary>
    /// Gets the name of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    string? name;

    /// <summary>
    /// Gets the reference of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    DocumentReference reference;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> create time of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    DateTimeOffset createTime;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> update time of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    DateTimeOffset updateTime;

    #endregion

    #region Initializers

    internal Document(string name, DocumentReference reference, DateTimeOffset createTime, DateTimeOffset updateTime)
    {
        this.name = name;
        this.reference = reference;
        this.createTime = createTime;
        this.updateTime = updateTime;
    }

    /// <summary>
    /// Creates an instance of <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="reference">
    /// The <see cref="DocumentReference"/> of the document.
    /// </param>
    public Document(DocumentReference reference)
    {
        this.reference = reference;
    }

    #endregion

    #region Methods



    #endregion
}

/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public partial class Document<T> : Document
     where T : class
{
    #region Properties

    /// <summary>
    /// Gets the <typeparamref name="T"/> model of the document.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    T? model;

    #endregion

    #region Initializers

    internal Document(string name, DocumentReference reference, T model, DateTimeOffset createTime, DateTimeOffset updateTime)
        : base(name, reference, createTime, updateTime)
    {
        this.model = model;
    }

    /// <summary>
    /// Creates an instance of <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="reference">
    /// The <see cref="DocumentReference"/> of the document.
    /// </param>
    /// <param name="model">
    /// The model of the document.
    /// </param>
    public Document(DocumentReference reference, T? model)
        : base (reference)
    {
        this.model = model;
    }

    #endregion

    #region Methods



    #endregion
}
