using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
public partial class Document : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Gets the name of the document node.
    /// </summary>
    public string? Name
    {
        get => name;
        internal set
        {
            if (!EqualityComparer<string?>.Default.Equals(name, value))
            {
                OnPropertyChanging();
                name = value;
                OnPropertyChanged();
            }
        }
    }
    string? name;

    /// <summary>
    /// Gets the reference of the document node.
    /// </summary>
    public DocumentReference Reference
    {
        get => reference;
        internal set
        {
            if (!EqualityComparer<DocumentReference>.Default.Equals(reference, value))
            {
                OnPropertyChanging();
                reference = value;
                OnPropertyChanged();
            }
        }
    }
    DocumentReference reference;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> create time of the document node.
    /// </summary>
    public DateTimeOffset CreateTime
    {
        get => createTime;
        internal set
        {
            if (!EqualityComparer<DateTimeOffset>.Default.Equals(createTime, value))
            {
                OnPropertyChanging();
                createTime = value;
                OnPropertyChanged();
            }
        }
    }
    DateTimeOffset createTime;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> update time of the document node.
    /// </summary>
    public DateTimeOffset UpdateTime
    {
        get => updateTime;
        internal set
        {
            if (!EqualityComparer<DateTimeOffset>.Default.Equals(updateTime, value))
            {
                OnPropertyChanging();
                updateTime = value;
                OnPropertyChanged();
            }
        }
    }
    DateTimeOffset updateTime;

    /// <summary>
    /// Gets the type of the document model.
    /// </summary>
    public virtual Type? Type { get => GetModel()?.GetType(); }

    /// <summary>
    /// Gets the fields of the document.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Fields { get; }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

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

    /// <summary>
    /// Raises the <see cref = "PropertyChanged"/> event.
    /// </summary>
    /// <param name = "propertyName">
    /// (optional) The name of the property that changed.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref = "PropertyChanging"/> event.
    /// </summary>
    /// <param name = "propertyName">
    /// (optional) The name of the property that changed.
    /// </param>
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
    }

    /// <summary>
    /// Raises <see cref = "PropertyChanged"/>.
    /// </summary>
    /// <param name = "e">
    /// The input <see cref = "PropertyChangedEventArgs"/> instance.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises <see cref = "PropertyChanging"/>.
    /// </summary>
    /// <param name = "e">
    /// The input <see cref = "PropertyChangingEventArgs"/> instance.
    /// </param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        PropertyChanging?.Invoke(this, e);
    }
}

/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
/// <typeparam name="TModel">
/// The type of the model of the document.
/// </typeparam>
public partial class Document<TModel> : Document
     where TModel : class
{
    /// <summary>
    /// Gets the <typeparamref name="TModel"/> model of the document.
    /// </summary>
    public TModel? Model
    {
        get => model;
        internal set
        {
            if (!EqualityComparer<TModel?>.Default.Equals(model, value))
            {
                OnPropertyChanging();
                model = value;
                OnPropertyChanged();
            }
        }
    }
    TModel? model;

    /// <inheritdoc/>
    public override Type? Type { get; } = typeof(TModel);

    /// <summary>
    /// Creates an instance of <see cref="Document{TModel}"/>.
    /// </summary>
    /// <param name="reference">
    /// The <see cref="DocumentReference"/> of the document.
    /// </param>
    /// <param name="model">
    /// The model of the document.
    /// </param>
    public Document(DocumentReference reference, TModel? model)
        : base (reference)
    {
        this.model = model;
    }
}
