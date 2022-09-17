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
/// Represents a partial document node of the firebase cloud firestore.
/// </summary>
[ObservableObject]
public partial class PartialDocument<T>
     where T : class
{
    #region Properties

    /// <summary>
    /// Gets the reference of the document node.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    DocumentReference reference;

    /// <summary>
    /// Gets the <typeparamref name="T"/> model of the document.
    /// </summary>
    [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
    T model;

    #endregion

    #region Initializers

    /// <summary>
    /// Creates an instance of <see cref="PartialDocument{T}"/>.
    /// </summary>
    /// <param name="reference">
    /// The reference of the document node.
    /// </param>
    /// <param name="model">
    /// The <typeparamref name="T"/> model of the document.
    /// </param>
    public PartialDocument(DocumentReference reference, T model)
    {
        this.reference = reference;
        this.model = model;
    }

    #endregion

    #region Methods



    #endregion
}
