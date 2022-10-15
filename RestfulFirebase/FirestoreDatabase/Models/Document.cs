using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
[ObservableObject]
public partial class Document
{
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

    /// <summary>
    /// Gets the type of the document model.
    /// </summary>
    public virtual Type? Type { get => GetModel()?.GetType(); }

    /// <summary>
    /// Gets the fields of the document.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Fields { get; }

    private readonly ConcurrentDictionary<string, object?> fields;

    /// <summary>
    /// Creates an instance of <see cref="Document{T}"/>.
    /// </summary>
    /// <param name="reference">
    /// The <see cref="DocumentReference"/> of the document.
    /// </param>
    public Document(DocumentReference reference)
    {
        this.reference = reference;

        fields = new();
        Fields = fields.AsReadOnly();
    }
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
    /// <summary>
    /// Gets the <typeparamref name="T"/> model of the document.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    T? model;

    /// <inheritdoc/>
    public override Type? Type { get; } = typeof(T);

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
}
