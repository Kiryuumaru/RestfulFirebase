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

    public async Task<string> Get(CancellationToken? token = null)
    {
        var url = string.Empty;
        var responseData = string.Empty;
        var statusCode = HttpStatusCode.OK;

        if (App.Config.CachedOfflineMode)
        {
            throw new OfflineModeException();
        }

        url = BuildUrl();

        try
        {
            CancellationToken invokeToken;

            if (token == null)
            {
                invokeToken = new CancellationTokenSource(App.Config.CachedDatabaseRequestTimeout).Token;
            }
            else
            {
                invokeToken = CancellationTokenSource.CreateLinkedTokenSource(token.Value, new CancellationTokenSource(App.Config.CachedDatabaseRequestTimeout).Token).Token;
            }

            using var client = await GetClient();
            var response = await client.GetAsync(url, invokeToken).ConfigureAwait(false);
            invokeToken.ThrowIfCancellationRequested();
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
