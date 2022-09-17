﻿using RestfulFirebase.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using System.Text.Json.Serialization;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Queries;

/// <summary>
/// The reference for documents.
/// </summary>
public class DocumentReference : Reference
{
    #region Properties

    /// <summary>
    /// Gets the ID of the collection reference.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the parent of the document reference.
    /// </summary>
    public CollectionReference Parent { get; }

    #endregion

    #region Initializers

    internal DocumentReference(CollectionReference parent, string documentId)
    {
        Id = documentId;
        Parent = parent;
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is DocumentReference reference &&
               base.Equals(obj) &&
               Id == reference.Id &&
               EqualityComparer<CollectionReference>.Default.Equals(Parent, reference.Parent);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1488852771;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
        hashCode = hashCode * -1521134295 + EqualityComparer<CollectionReference>.Default.GetHashCode(Parent);
        return hashCode;
    }

    /// <summary>
    /// Creates a collection reference <see cref="CollectionReference"/>.
    /// </summary>
    /// <param name="collectionId">
    /// The ID of the collection reference.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionId"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionId"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionReference Collection(string collectionId)
    {
        ArgumentNullException.ThrowIfNull(collectionId);

        return new CollectionReference(this, collectionId);
    }

    /// <summary>
    /// Creates a document <see cref="Models.Document{T}"/>.
    /// </summary>
    /// <param name="model">
    /// The model of the document
    /// </param>
    /// <returns>
    /// The <see cref="Models.Document{T}"/>.
    /// </returns>
    public Document<T> Document<T>(T? model = null)
        where T : class
    {
        return new Document<T>(this, model);
    }

    internal override string BuildUrlCascade(string projectId)
    {
        var url = Id;

        string parentUrl = Parent.BuildUrlCascade(projectId);
        if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
        {
            parentUrl += '/';
        }
        url = parentUrl + url;

        return url;
    }

    #endregion
}