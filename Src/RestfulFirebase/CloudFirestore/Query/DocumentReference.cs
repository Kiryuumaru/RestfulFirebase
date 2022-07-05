using RestfulFirebase.CloudFirestore.Models;
using RestfulFirebase.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace RestfulFirebase.CloudFirestore.Query;

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
    /// Gets the name of the document reference.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the parent of the document reference.
    /// </summary>
    public CollectionReference Parent { get; }

    #endregion

    #region Initializers

    internal DocumentReference(RestfulFirebaseApp app, CollectionReference parent, string documentId)
        : base(app)
    {
        Id = documentId;
        Parent = parent;

        Name = $"{parent.Name}/{documentId}";
    }

    #endregion

    #region Methods

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
        if (collectionId == null)
        {
            throw new ArgumentNullException(nameof(collectionId));
        }

        return new CollectionReference(App, this, collectionId);
    }

    /// <summary>
    /// Gets the JSON data of the document.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// The <see cref="JsonDocument"/> of the document.
    /// </returns>
    /// <exception cref="OfflineModeException">
    /// Throws if the app config <see cref="FirebaseConfig.OfflineMode"/> is set to <c>true</c>.
    /// </exception>
    public async Task<JsonDocument> GetAsync(CancellationToken? cancellationToken = null)
    {
        if (App.Config.OfflineMode)
        {
            throw new OfflineModeException();
        }

        var statusCode = HttpStatusCode.OK;

        var url = BuildUrl();

        try
        {
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
            }

            cancellationToken.Value.ThrowIfCancellationRequested();

            using var client = await GetClient();
            using var response = await client.GetAsync(url, cancellationToken.Value).ConfigureAwait(false);
            cancellationToken.Value.ThrowIfCancellationRequested();

            statusCode = response.StatusCode;
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            JsonDocument doc = await JsonDocument.ParseAsync(stream, RestfulFirebaseApp.DefaultJsonDocumentOptions, cancellationToken.Value).ConfigureAwait(false);
            cancellationToken.Value.ThrowIfCancellationRequested();

            return doc;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ExceptionHelpers.GetException(statusCode, ex);
        }
    }

    internal override string BuildUrl()
    {
        var url = BuildUrlSegment();

        string parentUrl = Parent.BuildUrl();
        if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
        {
            parentUrl += '/';
        }
        url = parentUrl + url;

        return url;
    }

    internal override string BuildUrlSegment()
    {
        return Id;
    }

    #endregion
}
