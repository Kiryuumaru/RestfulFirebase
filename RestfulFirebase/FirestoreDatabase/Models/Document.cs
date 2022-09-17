using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using RestfulFirebase.FirestoreDatabase.Queries;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RestfulFirebase.FirestoreDatabase.Models;

/// <summary>
/// Represents a document node of the firebase cloud firestore.
/// </summary>
/// <typeparam name="T">
/// The type of the model of the document.
/// </typeparam>
public partial class Document<T> : PartialDocument<T>
     where T : class
{
    #region Properties

    /// <summary>
    /// Gets the name of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    string name;

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

    internal Document(string name, DocumentReference reference, T model, DateTimeOffset createTime, DateTimeOffset updateTime)
        : base(reference, model)
    {
        this.name = name;
        this.createTime = createTime;
        this.updateTime = updateTime;
    }

    #endregion

    #region Methods



    #endregion
}
