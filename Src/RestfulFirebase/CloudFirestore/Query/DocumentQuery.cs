using RestfulFirebase.CloudFirestore.Models;
using RestfulFirebase.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.CloudFirestore.Query;

public class DocumentQuery : Query
{
    public string Name { get; }

    public CollectionQuery Parent { get; }

    public DocumentQuery(RestfulFirebaseApp app, CollectionQuery parent, string name)
        : base(app)
    {
        Name = name;
        Parent = parent;
    }

    public CollectionQuery Collection(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return new CollectionQuery(App, this, name);
    }

    public async Task<string> GetAsync(CancellationToken? cancellationToken = null)
    {
        if (App.Config.OfflineMode)
        {
            throw new OfflineModeException();
        }

        string responseData;
        var statusCode = HttpStatusCode.OK;

        var url = BuildUrl();

        try
        {
            if (cancellationToken == null)
            {
                cancellationToken = new CancellationTokenSource(App.Config.DatabaseRequestTimeout).Token;
            }

            using var client = await GetClient();
            var response = await client.GetAsync(url, cancellationToken.Value).ConfigureAwait(false);
            cancellationToken.Value.ThrowIfCancellationRequested();
            statusCode = response.StatusCode;
            responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            response.Dispose();



            return responseData;
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
        return Name;
    }
}
